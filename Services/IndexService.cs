using System.Text;
using Azure.Storage.Queues;
using Microsoft.Graph.Beta;
using Microsoft.KernelMemory;
using Microsoft.Teams.AI;
using Newtonsoft.Json;
using TeamsAIssistant.Config;

namespace TeamsAIssistant.Services
{
  public class IndexService(IConfiguration configuration,
    TeamsAdapter teamsAdapter,
    GraphClientServiceProvider? graphClientServiceProvider = null)
  {
    private readonly QueueClient _indexQueue = new(configuration.Get<ConfigOptions>()!.IndexQueue, "index-items");
    private readonly HttpClient _client = teamsAdapter.HttpClientFactory.CreateClient("VectorSearch");
    private readonly string? _searchEndpoint = configuration.Get<ConfigOptions>()!.SearchEndpoint;

    public async Task<SearchResult?>? Search(string query,
      IEnumerable<string>? indexes,
      IEnumerable<MemoryFilter> filters,
      double minRelevance = 0)
    {
      var response = await _client.PostAsJsonAsync(_searchEndpoint, new
      {
        Query = query,
        Indexes = indexes != null && indexes.Any() ? indexes : null,
        MemoryFilters = filters,
        MinRelevance = minRelevance,
      });

      if (!response.IsSuccessStatusCode || response.Content == null)
      {
        throw new HttpRequestException(response.ReasonPhrase);
      }

      return await response.Content.ReadFromJsonAsync<SearchResult>();
    }

    public async Task AddSiteToVectorIndex(string siteId, string? teamId = null)
    {
      if (graphClientServiceProvider == null)
      {
        throw new UnauthorizedAccessException();
      }

      var request = new
      {
        SiteId = siteId,
        TeamId = teamId,
        Token = graphClientServiceProvider.GetToken()
      };

      var base64EncodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));

      await _indexQueue.SendMessageAsync(base64EncodedMessage);
    }

    public async Task AddSimplicateVectorIndex()
    {
      var request = new
      {
        Simplicate = true
      };

      var base64EncodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));

      await _indexQueue.SendMessageAsync(base64EncodedMessage);
    }

    public async Task AddTeamToVectorIndex(string teamId, DateTimeOffset? modifiedAfter = null)
    {
      if (graphClientServiceProvider == null)
      {
        throw new UnauthorizedAccessException();
      }

      var request = new
      {
        TeamId = teamId,
        Token = graphClientServiceProvider.GetToken()
      };

      var base64EncodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));

      await _indexQueue.SendMessageAsync(base64EncodedMessage);
    }

    private async Task<List<string>> AddListToVectorIndex(GraphServiceClient graphServiceClient, string siteId, string listId, DateTimeOffset? modifiedAfter = null, string? teamId = null)
    {
      List<string> result = [];
      var list = await graphServiceClient.Sites[siteId].Lists[listId]
                 .GetAsync();

      if (list?.ListProp?.Template == "genericList")
      {
        var items = await graphServiceClient.Sites[siteId].Lists[listId].Items
              .GetAsync((config) =>
          {
            config.QueryParameters.Expand = ["fields"];
          });

        if (items != null)
        {
          var filteredItems = items.Value?
              .Where(r => !modifiedAfter.HasValue || r.LastModifiedDateTime >= modifiedAfter);

          var tags = new TagCollection{
                                        {"type", ["ListItem"]},
                                        {"listId", [listId]},
                                        {"siteId", [siteId]},
                                        {"year", [string.Empty]},
                                    };
          if (teamId != null)
          {
            tags.Add("teamId", [teamId]);
          }

          foreach (var doc in filteredItems ?? [])
          {
            string textValue = string.Empty;
            foreach (var f in doc?.Fields?.AdditionalData ?? new Dictionary<string, object>())
            {
              if (!f.Key!.StartsWith('@') && !f.Key!.StartsWith('_') && f.Value != null)
              {
                textValue += $"{f.Key?.ToString()}: {f.Value?.ToString()}";
              }
            }

            tags["year"] = [doc?.LastModifiedDateTime?.Year.ToString()];

            /*       var docId = await kernelMemory.ImportTextAsync(textValue,
                                                   tags: tags,
                                                   documentId: $"{siteId}-...-{listId}-...-{doc?.Id}",
                                                   steps: Microsoft.KernelMemory.Constants.PipelineWithSummary);

                   result.Add(docId);*/
          }
        }
      }

      return result;

    }

  }
}