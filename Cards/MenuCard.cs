using AdaptiveCards;
using System.Globalization;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using TeamsAIssistant.Models.Cards;

namespace TeamsAIssistant.AdaptiveCards
{
    public static class MenuCard
    {
        public const string MenuCardTemplate = "Resources/Cards/menuCardTemplate.json";
    }

    public class MenuCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
    {
        public Assistant? Assistant { get; set; }
        public string? BotName { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? Model { get; set; }
        public IEnumerable<string>? Tools { get; set; }
        public string? AdditionalInstructions { get; set; }
        public int MessageCount { get; set; }
        public string? Usage { get; set; }
        public IEnumerable<string>? AssistantPlugins { get; set; }
        public int FileCount { get; set; }
        public IEnumerable<string>? ConversationPlugins { get; set; }
        public IEnumerable<AdaptiveChoice>? Assistants { get; set; }
        public IEnumerable<string>? AllPlugins { get; set; }

        public int? SelectedSourcesCount { get; set; }

        public bool HasSources
        {
            get
            {
                return SelectedSourcesCount.HasValue;
            }
        }

        public bool HasAdditionalInstructions
        {
            get
            {
                return !string.IsNullOrEmpty(AdditionalInstructions);
            }
        }

        public bool ExportToolCalls { get; set; }
        public bool PrependDateTime { get; set; }
        public bool PrependUsername { get; set; }
        public string? SelectedToolNames => Tools != null && Tools.Any() ? string.Join(", ", Tools.Select(ToToolText)) : null;
        public string? SelectedToolValues => Tools != null && Tools.Any() ? string.Join(',', Tools) : null;

        public string? SelectedConversationPlugins => ConversationPlugins != null
            && ConversationPlugins.Any() ? string.Join(',', ConversationPlugins) : string.Empty;

        public string? AdditionalInstructionsText => GetResourceString("AdditionalInstructionsText");
        public string? SettingsText => GetResourceString("SettingsText");
        public string? MessagesText => GetResourceString("MessagesText");
        public string? ExportText => GetResourceString("ExportText");
        public string? NoText => GetResourceString("NoText");
        public string? YesText => GetResourceString("YesText");

        public string? ContextLengthText => GetResourceString("ContextLengthText");
        public string? AdditionalInstructionsContextText => GetResourceString("AdditionalInstructionsContextText");
        public string? UsageText => GetResourceString("UsageText");
        public string? OptionsText => GetResourceString("OptionsText");
        public string? SourceText => GetResourceString("SourceText");
        public string? ExtensionsText => GetResourceString("ExtensionsText");
        public string? KernelMemoryText => GetResourceString("KernelMemoryText");
        public string? TeamsText => GetResourceString("TeamsText");
        public string? ExportToolCallsText => GetResourceString("ExportToolCallsText");
        public string? AppendDateTimeText => GetResourceString("AppendDateTimeText");
        public string? AppendUserNameText => GetResourceString("AppendUserNameText");
        public string? ProjectsText => GetResourceString("ProjectsText");
        public string? SourcesText => GetResourceString("SourcesText");        
        public string? ResetConversationText => GetResourceString("ResetConversationText");
        public string? NoSourceText => GetResourceString("NoSourceText");
        public string? MaxCitationsText => GetResourceString("MaxCitationsText");
        public string? NoFilterText => GetResourceString("NoFilterText");
        public string? AdditionalInstructionsContextTitleText => GetResourceString("AdditionalInstructionsContextTitleText");
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