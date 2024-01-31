using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateHoursPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
        : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Hours")
    {

        [Action("Simplicate.SearchHours")]
        [Description("Search for hours in Simplicate")]
        [Parameter(name: "project.name", type: "string", description: "Name of the project")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "limit", type: "number", maximum: 100, description: "Item limit of the query")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
            description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
            description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchHours([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hours/hours");
        }

        [Action("Simplicate.SearchHourApprovals")]
        [Description("Search for hour approvals per day and per employee in Simplicate")]
        [Parameter(name: "employee.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "date][ge", type: "string", format: "date-time",
            description: "Date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "date][le", type: "string", format: "date-time",
            description: "Date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchHourApprovals([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hours/approval");
        }

        [Action("Simplicate.SearchHourSubmissions")]
        [Description("Search for hour submissions per day and per employee in Simplicate")]
        [Parameter(name: "employee_id", type: "string", description: "Id of the employee")]
        [Parameter(name: "date][ge", type: "string", format: "date-time",
            description: "Date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "date][le", type: "string", format: "date-time",
            description: "Date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchHourSubmissions([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "hours/submission");
        }

        [Action("Simplicate.AddNewHoursLeave")]
        [Description("Adds a new hours leave in Simplicate Hours")]
        [Parameter(name: "employee_id", type: "string", required: true, description: "Id of the employee")]
        [Parameter(name: "leave_type_id", type: "string", required: true, description: "Id of the leave type")]
        [Parameter(name: "start_date", type: "string", required: true, format: "date-time", description: "Start date of the leave")]
        [Parameter(name: "end_date", type: "string", required: true, format: "date-time", description: "End date of the leave")]
        [Parameter(name: "year", type: "number", required: true, description: "Year of the leave")]
        [Parameter(name: "hours", type: "number", required: true, description: "Hours of the leave")]
        [Parameter(name: "description", type: "string", multiline: true, description: "Description of the leave")]
        public async Task<string> AddNewHoursLeave([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var properties = GetActionParameters(actionName).ToList();
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "leave_type_id", "hrm/leavetype", "Leave type", "label");

            return await SendConfirmationCard(turnContext, actionName, parameters, properties);
        }

        [Submit]
        public Task SimplicateAddNewHoursLeaveSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitNewActionAsync(turnContext, turnState, "Simplicate.AddNewHoursLeave", data, "hours/leave", cancellationToken);
        }
    }
}
