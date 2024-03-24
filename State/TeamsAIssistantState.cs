using Microsoft.Teams.AI.AI.Planners.Experimental;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.State
{
    public class TeamsAIssistantState : AssistantsState
    {
        public List<string> Plugins
        {
            get => Conversation?.Get<List<string>>("conversation_plugins") ?? [];
            set => Conversation?.Set("conversation_plugins", value);
        }

        public bool? ExportToolCalls
        {
            get => Conversation?.Get<bool?>("export_tool_calls");
            set => Conversation?.Set("export_tool_calls", value);
        }

        public bool? CreateFunctionExports
        {
            get => User?.Get<bool?>("create_function_exports");
            set => User?.Set("create_function_exports", value);
        }

        public bool? PrependDateTime
        {
            get => User?.Get<bool?>("prepend_datetime");
            set => User?.Set("prepend_datetime", value);
        }

        public bool? PrependUsername
        {
            get => User?.Get<bool?>("prepend_username");
            set => User?.Set("prepend_username", value);
        }

        public string? AdditionalInstructions
        {
            get => User?.Get<string?>("additional_instructions");
            set => User?.Set("additional_instructions", value);
        }

        public List<string> SiteIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_site_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_site_indexes", value);
        }

        public List<string> DriveIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_drive_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_drive_indexes", value);
        }

        public List<string> SimplicateIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_simplicate_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_simplicate_indexes", value);
        }

        public List<string> DataverseIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_dataverse_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_dataverse_indexes", value);
        }

        public List<string> GraphIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_graph_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_graph_indexes", value);
        }

        public List<string> TeamIndexes
        {
            get => Conversation?.Get<List<string>>("kernelmemory_team_indexes") ?? [];
            set => Conversation?.Set("kernelmemory_team_indexes", value);
        }

        public List<string> YearFilters
        {
            get => Conversation?.Get<List<string>>("kernelmemory_year_filters") ?? [];
            set => Conversation?.Set("kernelmemory_year_filters", value);
        }

        public List<string> TypeFilters
        {
            get => Conversation?.Get<List<string>>("kernelmemory_type_filters") ?? [];
            set => Conversation?.Set("kernelmemory_type_filters", value);
        }

        public int? MaxCitations
        {
            get => (int?)User?.Get<long?>("assistant_max_citations");
            set => User?.Set("assistant_max_citations", (long?)value);
        }

        public int? ContextLength
        {
            get => (int?)User?.Get<long?>("assistant_context_length");
            set => User?.Set("assistant_context_length", (long?)value);
        }

        public bool? AdditionalInstructionsContext
        {
            get => User?.Get<bool?>("additional_instructions_context");
            set => User?.Set("additional_instructions_context", value);
        }

        public double MinRelevance
        {
            get => User?.Get<double?>("kernelmemory_min_relevance") ?? AIConstants.DefaultMinRelevance;
            set => User?.Set("kernelmemory_min_relevance", value);
        }

        public string? ThreadMessageHistory
        {
            get => Temp?.Get<string?>("thread_message_history");
            set => Temp?.Set("thread_message_history", value);
        }
    }
}
