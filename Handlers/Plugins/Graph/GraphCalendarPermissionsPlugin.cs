using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphCalendarPermissionsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Calendar Permissions")
    {
        [Action("MicrosoftGraph.ListCalendarPermissions")]
        [Description("Lists calendar permissions")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to get the calendar permissions from. Defaults to current user")]
        public Task<string> ListCalendarPermissions([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                      ? await graphClient.Users[value.ToString()].Calendar.CalendarPermissions
                          .GetAsync() : await graphClient.Me.Calendar.CalendarPermissions
                          .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateCalendarPermission")]
        [Description("Creates a calendar permissions")]
        [Parameter(name: "userId", type: "string", readOnly: true, visible: false, description: "User id of the user to cancel the calendar event from. Defaults to current user")]
        [Parameter(name: "mail", type: "string", required: true, description: "E-mail of the user to give permissions to")]
        [Parameter(name: "isInsideOrganization", type: "boolean", required: true, description: "True if the user in context (share recipient or delegate) is inside the same organization as the calendar owner")]
        [Parameter(name: "calendarRoleType", type: "string", enumValues: ["Read", "Write", "LimitedRead", "None", "FreeBusyRead", "Write", "DelegateWithPrivateEventAccess", "DelegateWithoutPrivateEventAccess"],
            required: true, description: "Current permission level of the calendar share recipient or delegate")]
        public Task<string> CreateCalendarPermission([ActionTurnContext] TurnContext turnContext,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateCalendarPermissionSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateCalendarPermission", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    CalendarPermission? result;

                    var requestBody = new CalendarPermission
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = jObject?["mail"]?.ToString(),
                        },
                        IsInsideOrganization = jObject?["isInsideOrganization"]?.ToObject<bool>(),
                        IsRemovable = true,
                        Role = Enum.Parse<CalendarRoleType>(jObject?["calendarRoleType"]?.ToString() ?? Enum.GetName(CalendarRoleType.None)!),
                    };

                    if (userId != null)
                    {
                        result = await graphClient.Users[userId].Calendar.CalendarPermissions.PostAsync(requestBody);
                    }
                    else
                    {
                        result = await graphClient.Me.Calendar.CalendarPermissions.PostAsync(requestBody);
                    }

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteCalendarPermission")]
        [Description("Deletes a calendar permission")]
        [Parameter(name: "userId", type: "string", readOnly: true, visible: false, description: "User id of the user to cancel the calendar event from. Defaults to current user")]
        [Parameter(name: "calendarPermissionId", type: "string", required: true, visible: false, description: "Id of the calendar permission")]
        public Task<string> DeleteCalendarPermission([ActionTurnContext] TurnContext turnContext,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var userId = parameters.TryGetValue("userId", out object? value) ? value.ToString() : "me";
                    var calendarPermissionId = parameters["calendarPermissionId"]?.ToString();

                    var permission = await graphClient.Users[userId].Calendar.CalendarPermissions[calendarPermissionId].GetAsync();
                    var emailAddress = permission?.EmailAddress?.Address ?? string.Empty;
                    var role = permission?.Role != null ? Enum.GetName(permission.Role.Value)! : string.Empty;

                    return [
                        (new ParameterAttribute(name: "EmailAddress", type: "string", readOnly: true), emailAddress),
                        (new ParameterAttribute(name: "Role", type: "string", readOnly: true), role)
                    ];
                });
        }

        [Submit]
        public Task DeleteCalendarPermissionSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteCalendarPermission", data,
             async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var itemId = jObject?["calendarPermissionId"]?.ToString();

                    if (userId != null)
                    {
                        await graphClient.Users[userId].Calendar.CalendarPermissions[itemId].DeleteAsync();
                    }
                    else
                    {
                        await graphClient.Me.Calendar.CalendarPermissions[itemId].DeleteAsync();
                    }

                    return "Calendar permission deleted";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdateCalendarPermission")]
        [Description("Updates a calendar permission")]
        [Parameter(name: "userId", type: "string", readOnly: true, visible: false, description: "User id of the user to cancel the calendar event from. Defaults to current user")]
        [Parameter(name: "calendarPermissionId", type: "string", required: true, visible: false, description: "Id of the calendar permission")]
        [Parameter(name: "calendarRoleType", type: "string", enumValues: ["Read", "Write", "LimitedRead", "None", "FreeBusyRead", "Write", "DelegateWithPrivateEventAccess", "DelegateWithoutPrivateEventAccess"],
            required: true, description: "Current permission level of the calendar share recipient or delegate")]
        public Task<string> UpdateCalendarPermission([ActionTurnContext] TurnContext turnContext,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var userId = parameters.TryGetValue("userId", out object? value) ? value.ToString() : "me";
                    var calendarPermissionId = parameters["calendarPermissionId"]?.ToString();

                    var permission = await graphClient.Users[userId].Calendar.CalendarPermissions[calendarPermissionId].GetAsync();
                    var emailAddress = permission?.EmailAddress?.Address ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "EmailAddress", type: "string", readOnly: true), emailAddress)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphUpdateCalendarPermissionSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateCalendarPermission", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var itemId = jObject?["calendarPermissionId"]?.ToString();
                    CalendarPermission? result;

                    var requestBody = new CalendarPermission
                    {
                        Role = Enum.Parse<CalendarRoleType>(jObject?["calendarRoleType"]?.ToString() ?? Enum.GetName(CalendarRoleType.None)!),
                    };

                    if (userId != null)
                    {
                        result = await graphClient.Users[userId].Calendar.CalendarPermissions[itemId].PatchAsync(requestBody);
                    }
                    else
                    {
                        result = await graphClient.Me.Calendar.CalendarPermissions[itemId].PatchAsync(requestBody);
                    }

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }
    }
}
