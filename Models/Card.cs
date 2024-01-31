
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Teams.AI.AI.OpenAI.Models;

namespace TeamsAIssistant.Models.Cards;

public class CardData(CultureInfo cultureInfo)
{
    public string? Header { get; set; }
    public string? SubTitle { get; set; }

    protected ResourceManager ResourceManager { get; } = new ResourceManager("AIssistant.Resources.Translations.Cards", Assembly.GetExecutingAssembly());
    public CultureInfo CultureInfo { get; set; } = cultureInfo;

    protected string? GetResourceString(string name)
    {
        return ResourceManager.GetString(name, CultureInfo);
    }

    public string? SaveText => GetResourceString("SaveText");
    public string? FilesText => GetResourceString("FilesText");
    public string? PluginsText => GetResourceString("PluginsText");
    public string? ToolsText => GetResourceString("ToolsText");
    public string? AssistantText => GetResourceString("AssistantText");
    public string? ExperimentalText => GetResourceString("ExperimentalText");
    public string? DeleteText => GetResourceString("DeleteText");
    public string? NoToolsText => GetResourceString("NoToolsText");
    public string? NoFilesText => GetResourceString("NoFilesText");
    public string? NoPluginsText => GetResourceString("NoPluginsText");
    public string? CodeInterpreterText => GetResourceString("CodeInterpreterText");
    public string? RetrievalText => GetResourceString("RetrievalText");

    protected string? ToToolText(string type)
    {
        return type switch
        {
            "retrieval" => GetResourceString("RetrievalText"),
            Tool.CODE_INTERPRETER_TYPE => GetResourceString("CodeInterpreterText"),
            _ => GetResourceString("FunctionText"),
        };
    }
}
