namespace TeamsAIssistant.Constants
{
    public static class SubmitActions
    {
        public const string ClearConversationVerb = "ClearConversation";
        public const string AssistantVerb = "Assistant";
        public const string ExtensionsVerb = "Extensions";
        public const string FilesVerb = "Files";
        public const string ConversationVerb = "Conversation";
        public const string UpdateAssistantVerb = "UpdateAssistant";
        public const string CloneAssistantVerb = "CloneAssistant";
        public const string DeleteAssistantVerb = "DeleteAssistant";
        public const string UpdateConversationVerb = "UpdateConversation";
        public const string UpdatePluginsVerb = "UpdatePlugins";
        public const string UpdateKernelMemoryVerb = "UpdateKernelMemory";
        public const string CommandsVerb = "Commands";
        public const string ExportVerb = "Export";
        public const string DeleteFileVerb = "DeleteFile";
        public const string DeleteAssistantFileVerb = "DeleteAssistantFile";
        public const string FileToAssistantVerb = "FileToAssistant";
        public const string AddToChatVerb = "AddToChat";
    }

    public static class AssistantForm
    {
        public const string AssistantId = "AssistantId";
        public const string DeleteAssistantId = "DeleteAssistantId";
        public const string ModelId = "ModelId";
        public const string Indexes = "Indexes";
        public const string Sites = "Sites";
        public const string Teams = "Teams";
        public const string Dataverse = "Dataverse";
        public const string Graph = "Graph";
        public const string MaxCitations = "MaxCitations";
        public const string ContextLength = "ContextLength";
        public const string MinRelevance = "MinRelevance";
        public const string Simplicate = "Simplicate";
        public const string Years = "Years";
        public const string Types = "Types";
        public const string DescriptionId = "DescriptionId";
        public const string AdditionalInstructionsId = "AdditionalInstructionsId";
        public const string NameId = "NameId";
        public const string InstructionId = "InstructionId";
        public const string MetadataId = "MetadataId";
        public const string Tools = "Tools";
        public const string ExportFunctionOutput = "ExportFunctionOutput";
        public const string AdditionalInstructionsContext = "AdditionalInstructionsContext";
        public const string PrependDateTime = "PrependDateTime";
        public const string PrependUsername = "PrependUsername";
        public const string ExportToolCalls = "ExportToolCalls";
        public const string Plugins = "Plugins";
        public const string Team = "Team";
        public const string Visibility = "Visibility";
        public const string FileId = "FileId";
        public const string FileIds = "FileIds";

    }

    public enum Visibility
    {
        Organization,
        Owners,
        Team
    }

    public static class AssistantMetadata
    {
        public const string Owners = "Owners";
        public const string Visibility = "Visibility";
        public const string Index = "Index";
        public const string Sites = "Sites";
        public const string Drives = "Drives";
        public const string Team = "Team";
        public const string Plugins = "Plugins";
    }

    public static class Auth
    {
        public const string Graph = "graph";
    }
}