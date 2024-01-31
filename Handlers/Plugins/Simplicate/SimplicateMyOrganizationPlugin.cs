using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateMyOrganizationPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "My Organizations")
    {

        [Action("Simplicate.SearchMyOrganizations")]
        [Description("Searches for my organization profiles in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of my organization")]
        public Task<string> SearchMyOrganizations([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/myorganizationprofile");
        }
    }
}
