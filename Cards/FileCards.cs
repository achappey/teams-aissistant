using System.Globalization;
using TeamsAIssistant.Models.Cards;

namespace TeamsAIssistant.AdaptiveCards;

public static class FileCards
{
    public const string FileCardTemplate = "Resources/Cards/fileCardTemplate.json";
    public const string FilesCardTemplate = "Resources/Cards/filesCardTemplate.json";

}

public class FileCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
{
    public string? Url { get; set; }
    public string? Filename { get; set; }
    public string? Status { get; set; }
    public string? FileNameText => GetResourceString("FileNameText");
    public string? FileText => GetResourceString("FileText");
    public string? OpenFileText => GetResourceString("OpenFileText");
}

public class FilesCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
{
    public string? AssistantName { get; set; }
    public List<Models.File>? AssistantFiles { get; set; }
    public List<Models.File>? ConversationFiles { get; set; }
    public bool IsAssistantOwner { get; set; }
    public bool ShowConversationFiles { get; set; }
    public string? ConversationText => GetResourceString("ConversationText");
    public string? AttachToAssistantText => GetResourceString("AttachToAssistantText");
    public string? ConversationFilesText => GetResourceString("ConversationFilesText");
}
