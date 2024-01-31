using AdaptiveCards;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Models.Cards;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using System.Globalization;
using Newtonsoft.Json;

namespace TeamsAIssistant.AdaptiveCards;

public static class AssistantCard
{
    public const string AssistantCardTemplate = "Resources/Cards/assistantCardTemplate.json";
}

public class AssistantCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
{
    public Assistant? Assistant { get; set; }

    public IEnumerable<string>? Tools
    {
        get
        {
            return Assistant != null && Assistant.Tools.Count != 0
                ? Assistant.Tools.Where(t => t.Type != Tool.FUNCTION_CALLING_TYPE).Select(a => a.Type)
                : null;
            ;
        }
    }

    public string? SelectedToolNames => Tools != null && Tools.Any() ? string.Join(", ", Tools.Select(r => ToToolText(r))) : null;
    public string? SelectedVisibilityName => ToVisibilityText(Visibility);
    public string? SelectedToolValues => Tools != null && Tools.Any() ? string.Join(',', Tools.Select(r => r)) : null;

    public string? SelectedTeam => Assistant?.GetTeam();

    public string? CreatedAt
    {
        get
        {
            return Assistant != null ? DateTime.FromFileTimeUtc(Assistant.CreatedAt).ToString("F") : string.Empty;
        }
    }
    
    public string? Description
    {
        get
        {
            return Assistant != null && Assistant.Description != null ? Assistant.Description : string.Empty;
        }
    }

    public string? OwnerNames { get; set; }
    public string? TeamName { get; set; }

    public string? Plugins
    {
        get
        {
            return Assistant?.GetPlugins();
        }
    }

    public string? Metadata
    {
        get
        {
            return JsonConvert.SerializeObject(Assistant?.Metadata, Formatting.Indented);
        }
    }

    public string? TeamId
    {
        get
        {
            return Assistant?.GetTeam();
        }
    }

    public string? Visibility
    {
        get
        {
            return Assistant?.GetVisibility() ?? string.Empty;
        }
    }

    public int? FileCount
    {
        get
        {
            return Assistant?.FileIds.Count;
        }
    }

     public string? FileCountText
    {
        get
        {
            return Assistant?.FileIds.Count.ToString();
        }
    }

    public bool IsOwner { get; set; }
    public bool IsAuthenticated { get; set; }
    public IEnumerable<AdaptiveChoice>? TeamChoices { get; set; }
    public IEnumerable<AdaptiveChoice>? PluginChoices { get; set; }
    public bool CanDelete { get; set; }

    public string? InstructionsText => GetResourceString("InstructionsText");
    public string? NameText => GetResourceString("NameText");
    public string? VisibilityText => GetResourceString("VisibilityText");
    public string? DescriptionText => GetResourceString("DescriptionText");
    public string? EditText => GetResourceString("EditText");
    public string? CloneText => GetResourceString("CloneText");
    public string? AdvancedText => GetResourceString("AdvancedText");
    public string? AreYouSureText => GetResourceString("AreYouSureText");
    public string? YesDeleteAssistantText => GetResourceString("YesDeleteAssistantText");
    public string? OwnersText => GetResourceString("OwnersText");
    public string? CreatedAtText => GetResourceString("CreatedAtText");
    public string? NoTeamText => GetResourceString("NoTeamText");
    public string? AdditionalInstructionsPlaceholderText => GetResourceString("AdditionalInstructionsPlaceholderText");
    public string? OrganizationText => GetResourceString("OrganizationText");

     protected string? ToVisibilityText(string? visibility)
    {
        return visibility switch
        {
            "Owners" => GetResourceString("OwnersText"),
            "Organization" => GetResourceString("OrganizationText"),
            "Team" => "Team",
            _ => "Organization"
        };
    }
}



