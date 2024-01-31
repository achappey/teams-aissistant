using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.SchoolHolidays
{
    public class ClearbitPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : PluginBase(driveRepository, proactiveMessageService, "Company Logos", "Clearbit", "Logo API", "v1")
    {

        [Action("Clearbit.GetCompanyLogo")]
        [Description("Gets a company logo by domain")]
        [Parameter(name: "domain", type: "string", required: true, description: "Domain of the company. For example google.com or microsoft.com")]
        [Parameter(name: "greyscale", type: "boolean", description: "Desaturates the image")]
        public async Task<string> GetCompanyLogo([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("domain", out var domain))
            {
                return "Domain parameter is missing.";
            }

            string greyscaleParam = "";
            if (parameters.ContainsKey("greyscale") && bool.TryParse(parameters["greyscale"].ToString(), out bool greyscale) && greyscale)
            {
                greyscaleParam = "&greyscale=true";
            }

            await SendFunctionCard(turnContext, actionName, parameters);
            await turnContext.SendActivityAsync("<a href=\"https://clearbit.com\">Logos provided by Clearbit</a>");

            return $"https://logo.clearbit.com/{domain}?size=256{greyscaleParam}";
        }
    }
}
