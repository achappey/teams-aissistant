using AdaptiveCards;
using System.Globalization;
using Microsoft.Graph.Beta.Models;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Models.Cards;

namespace TeamsAIssistant.AdaptiveCards
{
    public static class ExtensionsCard
    {
        public const string ExtensionsCardemplate = "Resources/Cards/extensionsCardTemplate.json";
    }

    public class ExtensionsCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
    {
        public bool IsAuthenticated { get; set; }
        public int? MaxCitations { get; set; }
        public int? ContextLength { get; set; }
        public bool? AdditionalInstructionsContext { get; set; }
        public IEnumerable<string>? Tools { get; set; }
        public IEnumerable<string>? AssistantPlugins { get; set; }
        public IEnumerable<string>? ConversationPlugins { get; set; }
        public IEnumerable<string>? AllPlugins { get; set; }

        public string? SelectedSites { get; set; }
        public string? SelectedTeams { get; set; }
        public string? SelectedYears { get; set; }
        public string? SelectedTypes { get; set; }
        public string? MinRelevance { get; set; }
        public int? FilterCount { get; set; }
        public string? SelectedSources { get; set; }

        public bool HasFilters
        {
            get
            {
                return FilterCount.HasValue && FilterCount > 0;
            }
        }

        public bool HasSources
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedSources);
            }
        }

        public IEnumerable<AdaptiveChoice>? SiteChoices
        {
            get
            {
                return Sites?.Select(t => new AdaptiveChoice() { Title = t.DisplayName, Value = t.Id?.Split(",").ElementAt(1) });
            }
        }

        public IEnumerable<AdaptiveChoice>? YearChoices
        {
            get
            {
                return Years?.Select(CardExtensions.ToAdaptiveChoice);
            }
        }

        public IEnumerable<AdaptiveChoice>? TeamChoices
        {
            get
            {
                return Teams?.Select(t => new AdaptiveChoice() { Title = t.DisplayName, Value = t.Id });
            }
        }

        public IEnumerable<AdaptiveChoice>? MinRelevanceChoices
        {
            get
            {
                return MinRelevances?.Select(CardExtensions.ToAdaptiveChoice);
            }
        }

        public IEnumerable<string>? MinRelevances
        {
            get
            {
                return Enumerable.Range(2, 7)
                                         .Select(i => i / 10.0)
                                         .Reverse()
                                         .Select(a => a.ToString(CultureInfo.InvariantCulture))
                                         .ToList();
            }
        }

        public IEnumerable<string>? Years
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                int startYear = currentYear - 10;
                int count = 11;

                return Enumerable.Range(startYear, count)
                    .Reverse()
                    .Select(r => r.ToString());
            }
        }

        public IEnumerable<Site>? Sites { get; set; }
        public IEnumerable<Team>? Teams { get; set; }
        public IEnumerable<AdaptiveChoice>? Dataverses { get; set; }
        public string? SelectedDataverses { get; set; }
        public string? SelectedGraphSources { get; set; }

        public IEnumerable<AdaptiveChoice>? SelectablePlugins
        {
            get
            {
                return AllPlugins?
                    .Where(y => AssistantPlugins == null || !AssistantPlugins.Any(u => u == y))
                    .Select(CardExtensions.ToAdaptiveChoice);
            }
        }

        public bool ExportFunctionOutput { get; set; }
        public string? SelectedSimplicateModules { get; set; }
        public string? SelectedConversationPlugins => ConversationPlugins != null
            && ConversationPlugins.Any() ? string.Join(',', ConversationPlugins) : string.Empty;

        public string? AdditionalInstructionsText => GetResourceString("AdditionalInstructionsText");
        public string? SettingsText => GetResourceString("SettingsText");
        public string? MessagesText => GetResourceString("MessagesText");
        public string? ExportText => GetResourceString("ExportText");
        public string? CreatedYearText => GetResourceString("CreatedYearText");
        public string? NoText => GetResourceString("NoText");
        public string? YesText => GetResourceString("YesText");
        public string? ContextLengthText => GetResourceString("ContextLengthText");
        public string? AdditionalInstructionsContextText => GetResourceString("AdditionalInstructionsContextText");
        public string? ShortContextText => GetResourceString("ShortContextText");
        public string? LongContextText => GetResourceString("LongContextText");
        public string? MinRelevanceText => GetResourceString("MinRelevanceText");
        public string? MediumContextText => GetResourceString("MediumContextText");
        public string? OptionsText => GetResourceString("OptionsText");
        public string? ExportPluginOutputText => GetResourceString("ExportPluginOutputText");
        public string? SitesText => GetResourceString("SitesText");
        public string? UsersText => GetResourceString("UsersText");
        public string? SourceText => GetResourceString("SourceText");
        public string? ExtensionsText => GetResourceString("ExtensionsText");
        public string? TeamsText => GetResourceString("TeamsText");
        public string? ProjectsText => GetResourceString("ProjectsText");
        public string? MaxCitationsPlaceholderText => GetResourceString("MaxCitationsPlaceholderText");
        public string? NoSourceText => GetResourceString("NoSourceText");
        public string? MaxCitationsText => GetResourceString("MaxCitationsText");
        public string? NoFilterText => GetResourceString("NoFilterText");
        public string? AdditionalInstructionsContextTitleText => GetResourceString("AdditionalInstructionsContextTitleText");
        public string? ChannelMessageText => GetResourceString("ChannelMessageText");
        public string? ListItemText => GetResourceString("ListItemText");
        public string? SitePageText => GetResourceString("SitePageText");
        public string? KernelMemoryText => GetResourceString("KernelMemoryText");
        public string? DriveItemText => GetResourceString("DriveItemText");
        public string? FilterText => GetResourceString("FilterText");
        public string? AdditionalInstructionsPlaceholderText => GetResourceString("AdditionalInstructionsPlaceholderText");
        public string? ActivePlugins
        {
            get
            {
                var selectedPluginsList = new List<string>();

                if (ConversationPlugins != null && ConversationPlugins.Any())
                {
                    selectedPluginsList.AddRange(ConversationPlugins);
                }

                if (AssistantPlugins != null && AssistantPlugins.Any())
                {
                    selectedPluginsList.AddRange(AssistantPlugins);
                }

                return selectedPluginsList.Count != 0 ? string.Join(", ", selectedPluginsList) : null;
            }
        }


    }
}