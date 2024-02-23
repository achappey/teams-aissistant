using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.KernelMemory;
using Microsoft.Teams.AI.AI.Tokenizers;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Models;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.DataSources
{
    public class KernelMemoryData(IndexService indexService,
        GraphClientServiceProvider? graphClientServiceProvider = null,
        SimplicateClientServiceProvider? simplicateClientServiceProvider = null)
    {
        private static readonly Dictionary<string, DateTimeOffset> ItemUpdates = [];

        private async Task<string?> GetCitationUrl(Citation citation, HttpClient? simplicateClient = null)
        {
            var splitted = citation.DocumentId.Split("-...-");
            var graphClient = graphClientServiceProvider!.GetAuthenticatedGraphClient();

            switch (citation.Index)
            {
                case "SitePage":
                    var page = await graphClient.Sites[splitted.ElementAt(0)].Pages[citation.GetPageId()].GetAsync();
                    return page?.WebUrl;
                case "ListItem":

                    if (citation.Partitions.Any(e => e.Tags.ContainsKey("simplicateModule")))
                    {
                        if (simplicateClient == null)
                        {
                            throw new Exception();
                        }

                        var module = citation.Partitions.FirstOrDefault(e => e.Tags.ContainsKey("simplicateModule"))?.Tags["simplicateModule"]?.FirstOrDefault();
                        var itemType = citation.DocumentId.Split(".---.").FirstOrDefault();
                        var itemId = citation.DocumentId.Replace(".---.", ":");
                        var response = await simplicateClient.GetAsync($"{module}/{itemType}/{itemId}");

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception(response.ReasonPhrase);
                        }

                        var item = await response.Content.ReadFromJsonAsync<SimplicateItemData>();

                        return item?.Data?.SimplicateUrl;
                    }
                    else
                    {
                        var item = await graphClient.Sites[splitted.ElementAt(0)].Lists[splitted.ElementAt(1)].Items[splitted.ElementAt(2)].GetAsync();
                        return item?.WebUrl;

                    }
                case "ChannelMessage":
                    var message = await graphClient.Teams[splitted.ElementAt(0)]
                        .Channels[citation.GetChannelId()]
                        .Messages[splitted.ElementAt(2)].GetAsync();
                    return message?.WebUrl;

                case "DriveItem":
                    var driveItem = await graphClient.Drives[citation.GetDriveId()].Items[citation.GetDriveItemId()].GetAsync();
                    return driveItem?.WebUrl;
                default:
                    return string.Empty;
            }
        }

        private static List<MemoryFilter> GetFilters(
            IEnumerable<string>? siteIndexes,
            IEnumerable<string>? teamIndexes,
            IEnumerable<string>? driveIndexes,
            IEnumerable<string>? simplicateIndexes,
            IEnumerable<string>? yearFilters)
        {
            List<MemoryFilter> filters = [];
            bool includeYearFilter = yearFilters?.Any() == true;
            if (includeYearFilter)
            {
                foreach (var year in yearFilters ?? [])
                {
                    var siteFilters = siteIndexes?.Select(a => MemoryFilters.ByTag("siteId", a).ByTag("year", year));
                    filters.AddRange(siteFilters ?? []);
                    var teamFilters = teamIndexes?.Select(a => MemoryFilters.ByTag("teamId", a).ByTag("year", year));
                    filters.AddRange(teamFilters ?? []);
                    var driveFilters = driveIndexes?.Select(a => MemoryFilters.ByTag("driveId", a).ByTag("year", year));
                    filters.AddRange(driveFilters ?? []);
                    var simplicateFilters = simplicateIndexes?.Select(a => MemoryFilters.ByTag("simplicateModule", a).ByTag("year", year));
                    filters.AddRange(simplicateFilters ?? []);
                }
            }
            else
            {
                filters.AddRange(siteIndexes?.Select(a => MemoryFilters.ByTag("siteId", a)) ?? []);
                filters.AddRange(teamIndexes?.Select(a => MemoryFilters.ByTag("teamId", a)) ?? []);
                filters.AddRange(driveIndexes?.Select(a => MemoryFilters.ByTag("driveId", a)) ?? []);
                filters.AddRange(simplicateIndexes?.Select(a => MemoryFilters.ByTag("simplicateModule", a)) ?? []);
            }

            return filters;
        }

        private static async void UpdateIndexes(IEnumerable<string> indexes, Func<string, Task> addIndexFunc)
        {
            if (indexes == null) return;

            foreach (var index in indexes)
            {
                if (!ItemUpdates.TryGetValue(index, out DateTimeOffset value) || value < DateTime.Now.AddHours(-1))
                {
                    await addIndexFunc(index);

                    ItemUpdates[index] = DateTime.Now;
                }
            }
        }

        public async Task<(string context, List<Citation>? citations)> RenderDataAsync(string query,
            TeamsAIssistantState memory,
            ITurnContext context,
            ITokenizer tokenizer,
            int maxTokens)
        {
            List<Citation>? citations = [];

            (string context, List<Citation>? citations) noContext = (
                    context: "No context",
                    citations: null
                );

            if (memory.Temp.Input is not string ask)
            {
                return noContext;
            }

            var filters = GetFilters(memory.SiteIndexes, memory.TeamIndexes, memory.DriveIndexes, memory.SimplicateIndexes, memory.YearFilters);

            var results = await indexService.Search(query,
                indexes: memory.TypeFilters.Count != 0 ? memory.TypeFilters : null,
                filters: filters,
                minRelevance: memory.MinRelevance)!;

            if (results == null || results.NoResult)
            {
                return noContext;
            }

            var lastUpdated = results.Results
                .SelectMany(a => a.Partitions.Select(z => z.LastUpdate))
                .OrderByDescending(a => a)
                .FirstOrDefault();

            if (lastUpdated <= DateTime.Now.AddDays(-1) && graphClientServiceProvider != null)
            {
                UpdateIndexes(memory.SiteIndexes, async site => await indexService.AddSiteToVectorIndex(site));
                UpdateIndexes(memory.TeamIndexes, async team => await indexService.AddTeamToVectorIndex(team));
                UpdateIndexes(memory.SimplicateIndexes, async team => await indexService.AddSimplicateVectorIndex());
            }

            int length = 0;
            StringBuilder output = new();
            string connector = "";
            bool maxTokensReached = false;

            var client = simplicateClientServiceProvider != null ?
                 await simplicateClientServiceProvider.GetAuthenticatedSimplicateClient(context.Activity.From.AadObjectId) : null;

            foreach (Citation citation in results?.Results ?? [])
            {
                try
                {
                    citation.SourceUrl ??= await GetCitationUrl(citation, client);
                }
                catch (Exception)
                {
                    continue;
                }

                StringBuilder doc = new();
                doc.Append($"{connector}###\n");
                length += tokenizer.Encode($"{connector}###\n").Count;
                length += tokenizer.Encode("###\n").Count;

                length += tokenizer.Encode($"\nUrl:{citation.SourceUrl}\n").Count;

                bool contentAdded = false;
                var partition = citation.Partitions.FirstOrDefault(r => r.Tags.Any(a => a.Key == "__synth"));

                if (partition != null)
                {
                    int partitionLength = tokenizer.Encode(partition.Text).Count;
                    int remainingTokens = maxTokens - (length + partitionLength);
                    if (remainingTokens < 0)
                    {
                        maxTokensReached = true;
                        break;
                    }
                    length += partitionLength;
                    doc.Append($"{partition.Text}\n");
                    contentAdded = true;

                    if (contentAdded)
                    {
                        citations.Add(citation);
                        doc.Append($"\nUrl:{citation.SourceUrl}\n");
                        doc.Append("###\n");
                        output.Append(doc);
                        connector = "\n\n";
                    }

                    if (maxTokensReached)
                    {
                        break;
                    }
                }
            }

            if (!maxTokensReached)
            {
                var validResults = results?.Results.Where(r => !string.IsNullOrEmpty(r.SourceUrl));
                foreach (Citation citation in validResults ?? [])
                {
                    StringBuilder doc = new();
                    doc.Append($"{connector}###\n");
                    length += tokenizer.Encode($"{connector}###\n").Count;
                    length += tokenizer.Encode("###\n").Count;

                    if (!citations.Any(f => f.DocumentId == citation.DocumentId))
                    {
                        length += tokenizer.Encode($"\nUrl:{citation.SourceUrl}\n").Count;
                    }

                    var otherPartitions = citation.Partitions.Where(r => !r.Tags.Any(a => a.Key == "__synth"));

                    foreach (var partition in otherPartitions)
                    {
                        int partitionLength = tokenizer.Encode(partition.Text).Count;
                        int remainingTokens = maxTokens - (length + partitionLength);
                        if (remainingTokens < 0)
                        {
                            maxTokensReached = true;
                            break;
                        }
                        length += partitionLength;
                        doc.Append($"{partition.Text}\n");
                    }

                    if (!citations.Any(f => f.DocumentId == citation.DocumentId))
                    {
                        citations.Add(citation);
                        doc.Append($"\nUrl:{citation.SourceUrl}\n");
                    }

                    doc.Append("###\n");
                    output.Append(doc);
                    connector = "\n\n";

                    if (maxTokensReached)
                    {
                        break;
                    }
                }
            }

            return (
                    context: output.ToString(),
                    citations
                );
        }
    }
}