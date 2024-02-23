using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.SchoolHolidays
{
    public class TheColorAPIPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter) 
        : PluginBase(driveRepository, proactiveMessageService, "Colors", "GitHub", "The Color API", "v1")
    {
        private readonly HttpClient client = teamsAdapter.GetDefaultClient($"https://www.thecolorapi.com/", "TheColorAPI");

        [Action("GitHub.GetColorIdentification")]
        [Description("Return available identifying information on the given color")]
        [Parameter(name: "hex", type: "string", required: true, description: "Hex color code")]
        public Task<string> GetColorIdentification([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ProcessColorRequest(turnContext, turnState, actionName, parameters, "id");
        }

        [Action("GitHub.GetColorScheme")]
        [Description("Return a generated scheme for the provided seed color and optional mode.")]
        [Parameter(name: "hex", type: "string", required: true, description: "Hex color code")]
        [Parameter(name: "mode", type: "string", description: "Define mode by which to generate the scheme from the seed color",
            enumValues: ["monochrome", "monochrome-dark", "monochrome-light", "analogic", "complement", "analogic-complement", "triad", "quad"])]
        public Task<string> GetColorScheme([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ProcessColorRequest(turnContext, turnState, actionName, parameters, "scheme");
        }

        private async Task<string> ProcessColorRequest(ITurnContext turnContext, TeamsAIssistantState turnState, string actionName, Dictionary<string, object> parameters, string apiEndpoint)
        {
            if (!parameters.TryGetValue("hex", out var hex))
            {
                return "Hex parameter is missing.";
            }

            string hexString = hex.ToString() ?? string.Empty;
            hexString = CleanHexString(hexString);

            string queryString = BuildQueryString(parameters, hexString, apiEndpoint);

            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            using var response = await client.GetAsync(queryString);

            if (!response.IsSuccessStatusCode)
            {
                return response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage;
            }

            var result = await response.Content.ReadAsStringAsync();
            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, result, cardId);

            return result;
        }

        private static string CleanHexString(string hexString)
        {
            return hexString.StartsWith('#') ? hexString[1..] : hexString;
        }

        private static string BuildQueryString(Dictionary<string, object> parameters, string hexString, string apiEndpoint)
        {
            string queryString = $"{apiEndpoint}?hex={hexString}";

            if (apiEndpoint == "scheme" && parameters.TryGetValue("mode", out var mode) && mode is string modeString && !string.IsNullOrEmpty(modeString))
            {
                queryString += $"&mode={modeString}";
            }

            return queryString;
        }
    }
}
