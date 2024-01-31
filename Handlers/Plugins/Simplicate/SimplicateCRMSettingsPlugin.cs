using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateCRMSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
        : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "CRM Settings")
    {

        [Action("Simplicate.SearchCRMGenders")]
        [Description("Search for CRM genders in Simplicate")]
        public Task<string> SearchCRMGenders([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/gender");
        }

        [Action("Simplicate.SearchCRMIndustryTypes")]
        [Description("Search for CRM industry types in Simplicate")]
        public Task<string> SearchCRMIndustries([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/industry");
        }

        [Action("Simplicate.SearchCRMInterests")]
        [Description("Search for CRM interests in Simplicate")]
        public Task<string> SearchCRMInterests([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/interests");
        }

        [Action("Simplicate.SearchCRMRelationTypes")]
        [Description("Search for CRM relation types in Simplicate")]
        public Task<string> SearchCRMRelationTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/relationtype");
        }

        [Action("Simplicate.SearchCRMDocumentTypes")]
        [Description("Search for CRM document types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the document types")]
        [Parameter(name: "description", type: "string", description: "Description of the document types")]
        public Task<string> SearchCRMDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/documenttype");
        }
    }
}
