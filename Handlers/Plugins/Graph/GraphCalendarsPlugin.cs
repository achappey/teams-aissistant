using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphCalendarsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Calendars")
    {
        [Action("MicrosoftGraph.ListCalendars")]
        [Description("Lists calendars from a user")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to get the calendars from. Defaults to current user")]
        public Task<string> ListCalendars([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                      ? await graphClient.Users[value.ToString()].Calendars
                          .GetAsync() : await graphClient.Me.Calendars
                          .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.UpdateCalendar")]
        [Description("Update the properties of a users' calendar object")]
        [Parameter(name: "userId", type: "string", visible: false, readOnly: true, description: "User id of the user to accept the calendar event from. Defaults to current user")]
        [Parameter(name: "calendarId", type: "string", required: true, visible: false, description: "Id of the calendar")]
        [Parameter(name: "name", type: "string", required: true, description: "New name of the calendar")]
        [Parameter(name: "color", type: "string", required: true, description: "New color of the calendar",
            enumValues: ["LightBlue", "LightGreen", "LightOrange", "LightGray", "LightYellow", "LightTeal", "LightPink", "LightBrown", "LightRed", "MaxColor", "Auto"])]
        public Task<string> UpdateCalendar([ActionTurnContext] TurnContext turnContext,
                                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphUpdateCalendarSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateCalendar", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var calendarId = jObject?["calendarId"]?.ToString();

                    var requestBody = new Calendar
                    {
                        Name = jObject?["name"]?.ToString(),
                        Color = Enum.Parse<CalendarColor>(jObject?["color"]?.ToString() ?? Enum.GetName(CalendarColor.Auto)!),
                    };

                    Calendar? calendar;

                    if (userId != null)
                    {
                        calendar = await graphClient.Users[userId].Calendars[calendarId].PatchAsync(requestBody);
                    }
                    else
                    {
                        calendar = await graphClient.Me.Calendars[calendarId].PatchAsync(requestBody);
                    }

                    return JsonConvert.SerializeObject(calendar);
                }, cancellationToken);
        }

    }
}
