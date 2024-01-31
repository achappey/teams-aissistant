using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphCalendarEventsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Calendar Events")
    {
        [Action("MicrosoftGraph.SearchCalendarEvents")]
        [Description("Search calendar events of a user")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to get the calendar events from. Defaults to current user")]
        [Parameter(name: "start", type: "string", required: true, description: "The start date and time of the event in yyyy-MM-ddThh:mm:ss format")]
        [Parameter(name: "end", type: "string", required: true, description: "The end date and time of the event")]
        public Task<string> SearchCalendarEvents([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                      ? await graphClient.Users[value.ToString()].Events
                          .GetAsync((requestConfiguration) =>
                              {
                                  requestConfiguration.QueryParameters.Filter = $"start/dateTime ge '{parameters["start"]}' and end/dateTime le '{parameters["end"]}'";
                              }) : await graphClient.Me.Events
                          .GetAsync((requestConfiguration) =>
                              {
                                  requestConfiguration.QueryParameters.Filter = $"start/dateTime ge '{parameters["start"]}' and end/dateTime le '{parameters["end"]}'";
                              });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CancelEvent")]
        [Description("Cancels an event")]
        [Parameter(name: "userId", type: "string", visible: false, readOnly: true,
             description: "User id of the user to cancel the calendar event from. Defaults to current user")]
        [Parameter(name: "eventId", type: "string", required: true, visible: false, description: "Id of the event")]
        [Parameter(name: "comment", type: "string", required: true, multiline: true, description: "Comments")]
        public Task<string> CancelEvent([ActionTurnContext] TurnContext turnContext,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                   async (GraphServiceClient graphClient) =>
                   {
                       var eventId = parameters["eventId"]?.ToString();
                       var userId = parameters.TryGetValue("userId", out object? value) ? value.ToString() : null;
                       var item = userId != null
                                  ? await graphClient.Users[userId].Events[eventId].GetAsync()
                                  : await graphClient.Me.Events[eventId].GetAsync();

                       var subject = item?.Subject ?? string.Empty;
                       var bodyPreview = item?.BodyPreview ?? string.Empty;
                       var start = item?.Start?.DateTime?.ToString() ?? string.Empty;
                       var end = item?.End?.DateTime?.ToString() ?? string.Empty;

                       return [
                        (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                        (new ParameterAttribute(name: "Start", type: "string", readOnly: true), start),
                        (new ParameterAttribute(name: "End", type: "string", readOnly: true), end),
                        (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview)
                       ];
                   });
        }

        [Submit]
        public Task MicrosoftGraphCancelEventSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CancelEvent", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();

                    if (userId != null)
                    {
                        await graphClient.Users[userId].Events[jObject?["eventId"]?.ToString()].Cancel.PostAsync(new()
                        {
                            Comment = jObject?["comment"]?.ToString()
                        });
                    }
                    else
                    {
                        await graphClient.Me.Events[jObject?["eventId"]?.ToString()].Cancel.PostAsync(new()
                        {
                            Comment = jObject?["comment"]?.ToString()
                        });
                    }

                    return "Event cancelled";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.AcceptEvent")]
        [Description("Accepts an event")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to accept the calendar event from. Defaults to current user")]
        [Parameter(name: "eventId", type: "string", required: true, visible: false, description: "Id of the event")]
        [Parameter(name: "comment", type: "string", required: true, multiline: true, description: "Comments")]
        [Parameter(name: "sendResponse ", type: "boolean", required: true, description: "Sends a response")]
        public Task<string> AcceptEvent([ActionTurnContext] TurnContext turnContext,
                        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var eventId = parameters["eventId"]?.ToString();
                    var userId = parameters.TryGetValue("userId", out object? value) ? value.ToString() : null;
                    var item = userId != null
                            ? await graphClient.Users[userId].Events[eventId].GetAsync()
                            : await graphClient.Me.Events[eventId].GetAsync();

                    var subject = item?.Subject ?? string.Empty;
                    var bodyPreview = item?.BodyPreview ?? string.Empty;
                    var start = item?.Start?.DateTime?.ToString() ?? string.Empty;
                    var end = item?.End?.DateTime?.ToString() ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                        (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview),
                        (new ParameterAttribute(name: "Start", type: "string", readOnly: true), start),
                        (new ParameterAttribute(name: "End", type: "string", readOnly: true), end)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphAcceptEventSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.AcceptEvent", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var eventId = jObject?["eventId"]?.ToString();
                    var comment = jObject?["comment"]?.ToString();
                    var sendResponse = jObject?["sendResponse"]?.ToObject<bool>();

                    if (userId != null)
                    {
                        await graphClient.Users[userId].Events[eventId].Accept.PostAsync(new()
                        {
                            Comment = comment,
                            SendResponse = sendResponse,
                        });
                    }
                    else
                    {
                        await graphClient.Me.Events[eventId].Accept.PostAsync(new()
                        {
                            Comment = comment,
                            SendResponse = sendResponse,
                        });
                    }

                    return "Event accepted";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeclineEvent")]
        [Description("Declined an event, optional with a new proposed date time")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to accept the calendar event from. Defaults to current user")]
        [Parameter(name: "eventId", type: "string", required: true, visible: false, description: "Id of the event")]
        [Parameter(name: "comment", type: "string", required: true, multiline: true, description: "Comments")]
        [Parameter(name: "sendResponse ", type: "boolean", required: true, description: "Sends a response")]
        [Parameter(name: "start", type: "string", format: "date-time", description: "The new proposed start date and time of the event in yyyy-MM-ddThh:mm:ss format")]
        [Parameter(name: "end", type: "string", format: "date-time", description: "The new proposed end date and time of the event")]
        public Task<string> DeclineEvent([ActionTurnContext] TurnContext turnContext,
                                [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                   async (GraphServiceClient graphClient) =>
                   {
                       var eventId = parameters["eventId"]?.ToString();
                       var userId = parameters.TryGetValue("userId", out object? value) ? value.ToString() : null;
                       var item = userId != null
                                  ? await graphClient.Users[userId].Events[eventId].GetAsync()
                                  : await graphClient.Me.Events[eventId].GetAsync();

                       var subject = item?.Subject ?? string.Empty;
                       var bodyPreview = item?.BodyPreview ?? string.Empty;
                       var start = item?.Start?.DateTime?.ToString() ?? string.Empty;
                       var end = item?.End?.DateTime?.ToString() ?? string.Empty;

                       return [
                            (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                            (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview),
                            (new ParameterAttribute(name: "Start", type: "string", readOnly: true), start),
                            (new ParameterAttribute(name: "End", type: "string", readOnly: true), end)
                       ];
                   });
        }

        [Submit]
        public Task MicrosoftGraphDeclineEventSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeclineEvent", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var eventId = jObject?["eventId"]?.ToString();
                    var comment = jObject?["comment"]?.ToString();
                    var sendResponse = jObject?["sendResponse"]?.ToObject<bool>();
                    var start = jObject?["start"]?.ToString();
                    var end = jObject?["end"]?.ToString();

                    if (userId != null)
                    {
                        await graphClient.Users[userId].Events[eventId].Decline.PostAsync(new()
                        {
                            Comment = comment,
                            SendResponse = sendResponse,
                            ProposedNewTime = start != null && end != null ? new Microsoft.Graph.Beta.Models.TimeSlot()
                            {
                                Start = start.ToTimeZone(),
                                End = end.ToTimeZone()
                            } : null
                        });
                    }
                    else
                    {
                        await graphClient.Me.Events[eventId].Decline.PostAsync(new()
                        {
                            Comment = comment,
                            SendResponse = sendResponse,
                            ProposedNewTime = start != null && end != null ? new Microsoft.Graph.Beta.Models.TimeSlot()
                            {
                                Start = start.ToTimeZone(),
                                End = end.ToTimeZone()
                            } : null
                        });
                    }

                    return "Event declined";
                }, cancellationToken);
        }

    }
}
