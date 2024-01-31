using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateHoursSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Hours Settings")
    {

        [Action("Simplicate.SearchHourTypes")]
        [Description("Search for hour types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the hour type")]
        public Task<string> SearchHourTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hours/hourstype");
        }

        [Action("Simplicate.SearchHourApprovalStatus")]
        [Description("Search for hour approval status in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the hour approval status")]
        public Task<string> SearchHourApprovalStatus([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hours/approvalstatus");
        }

    }
}
