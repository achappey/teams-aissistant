using Microsoft.Teams.AI.AI.Planners.Experimental;

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
    }
}
