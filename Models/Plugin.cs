using System.Text.Json.Serialization;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using TeamsAIssistant.State;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Models;

public class Plugin
{
    [JsonPropertyName("name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name
    {
        get
        {
            return $"{Publisher} {DisplayName}";
        }
    }

    [JsonPropertyName("actions")]
    public IEnumerable<PluginAction>? Actions { get; set; }

    [JsonPropertyName("submits")]
    public IEnumerable<(string name, ActionSubmitHandler<TeamsAIssistantState> handler)>? Submits { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("api_name")]
    public string? ApiName { get; set; }

    [JsonPropertyName("api_version")]
    public string? ApiVersion { get; set; }

}

public class PluginAction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tool")]
    public Tool Tool { get; set; } = null!;

    [JsonPropertyName("parameters")]
    public IEnumerable<ParameterAttribute>? Parameters { get; set; }
}

