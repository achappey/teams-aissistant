using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateProjectSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Projects Settings")
    {

        [Action("Simplicate.SearchProjectPurchaseTypes")]
        [Description("Search for project purchase types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the purchase type")]
        public Task<string> SearchProjectPurchaseTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/purchasetype");
        }

        [Action("Simplicate.SearchProjectStatus")]
        [Description("Search for project status in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the project status")]
        public Task<string> SearchProjectStatus([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/projectstatus");
        }

        [Action("Simplicate.SearchProjectAssignmentStatus")]
        [Description("Search for project assignment status in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the assignment status")]
        public Task<string> SearchAssignmentStatus([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/assignmentstatus");
        }

        [Action("Simplicate.GetProjectDocumentTypes")]
        [Description("Gets project document types in Simplicate")]
        public Task<string> GetProjectDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/documenttype");
        }

    }
}
