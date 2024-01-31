using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateHRMPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "HRM")
    {

        [Action("Simplicate.SearchEmployees")]
        [Description("Search for employees in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the employee")]
        [Parameter(name: "function", type: "string", description: "Function of the employee")]
        public Task<string> SearchEmployees([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/employee");
        }

        [Action("Simplicate.SearchTimetables")]
        [Description("Search for timetables in Simplicate")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
            description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
            description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchTimetables([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/timetable");
        }

        [Action("Simplicate.SearchLeaves")]
        [Description("Search for leaves in Simplicate")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
            description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
            description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchLeaves([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/leave");
        }

        [Action("Simplicate.SearchLeaveBalances")]
        [Description("Search for leave balances in Simplicate")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "year", type: "number", description: "Year of the leave balance")]
        public Task<string> SearchLeaveBalances([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/leavebalance");
        }

        [Action("Simplicate.SearchAbsences")]
        [Description("Search for absences in Simplicate")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
            description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
            description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchAbsences([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/absence");
        }

        [Action("Simplicate.SearchHRMDocuments")]
        [Description("Search for HRM documents in Simplicate")]
        [Parameter(name: "title", type: "string", description: "Title of the HRM document")]
        [Parameter(name: "description", type: "string", description: "Description of the HRM document")]
        [Parameter(name: "document_type.label", type: "string", description: "Label of the document type")]
        [Parameter(name: "created_by.name", type: "string", description: "Name of the created by")]
        public Task<string> SearchHRMDocuments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hrm/document", "-created_at");
        }
    }
}
