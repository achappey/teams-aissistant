using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateHRMSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "HRM Settings")
    {

        [Action("Simplicate.SearchHRMAbsenceTypes")]
        [Description("Search for HRM absence types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the absence type")]
        public Task<string> SearchHRMAbsenceTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/absencetype");
        }

        [Action("Simplicate.SearchHRMLeaveTypes")]
        [Description("Search for HRM leave types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the leave type")]
        public Task<string> SearchHRMLeaveTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/leavetype");
        }

        [Action("Simplicate.SearchHRMContractTypes")]
        [Description("Search for HRM contract types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the contract type")]
        public Task<string> SearchHRMContractTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/contracttype");
        }

        [Action("Simplicate.SearchHRMEmploymentTypes")]
        [Description("Search for HRM employment types in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the employment type")]
        public Task<string> SearchHRMEmploymentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/employmenttype");
        }

        [Action("Simplicate.SearchHRMCivilStatus")]
        [Description("Search for HRM civil status in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the civil status")]
        public Task<string> SearchHRMCivilStatus([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/civilstatus");
        }

        [Action("Simplicate.SearchHRMTeams")]
        [Description("Search for HRM Teams in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the team")]
        public Task<string> SearchHRMTeams([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/team");
        }

        [Action("Simplicate.GetHRMDocumentTypes")]
        [Description("Gets HRM document types in Simplicate")]
        public Task<string> GetHRMDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/documenttype");
        }

    }
}
