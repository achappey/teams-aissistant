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
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTodoTasksPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Todo Tasks")
    {

        [Action("MicrosoftGraph.GetMyTodoTasks")]
        [Description("Get a list of the todo tasks by task list id")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        public Task<string> GetMyTodoTasks([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Todo.Lists[parameters["taskListId"]?.ToString()].Tasks
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateTodoTask")]
        [Description("Creates a new Todo task")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        [Parameter(name: "title", type: "string", required: true, description: "Title of the task")]
        [Parameter(name: "content", type: "string", required: true, description: "Content of the task")]
        [Parameter(name: "bodyType", type: "string", required: true, enumValues: ["Text", "Html"], description: "Body type of the content")]
        [Parameter(name: "status", type: "string", enumValues: ["WaitingOnOthers", "Completed", "Deferred", "InProgress", "NotStarted"], description: "Status of the task")]
        [Parameter(name: "importance", type: "string", enumValues: ["Low", "Normal", "High"], description: "Importance of the task")]
        [Parameter(name: "categories", type: "string", description: "Comma separated list of categories associated with the task. Each category corresponds to the displayName property of an outlookCategory that the user has defined.")]
        [Parameter(name: "startDateTime", type: "string", format: "date-time", description: "The date at which the task is scheduled to start in yyyy-MM-ddThh:mm:ss format")]
        [Parameter(name: "reminderDateTime", type: "string", format: "date-time", description: "The date and time for a reminder alert of the task to occur")]
        [Parameter(name: "dueDateTime", type: "string", format: "date-time", description: "The date that the task is to be finished")]
        public Task<string> CreateTodoTask([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateTodoTaskSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTodoTask", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var startDateTime = jObject?["startDateTime"]?.ToString();
                var dueDateTime = jObject?["dueDateTime"]?.ToString();
                var reminderDateTime = jObject?["reminderDateTime"]?.ToString();

                var requestBody = new TodoTask
                {
                    Title = jObject?["title"]?.ToString(),
                    StartDateTime = startDateTime?.ToTimeZone(),
                    DueDateTime = dueDateTime?.ToTimeZone(),
                    ReminderDateTime = reminderDateTime?.ToTimeZone(),
                    Importance = jObject != null && jObject.ContainsKey("importance")
                        ? Enum.Parse<Microsoft.Graph.Beta.Models.Importance>(jObject?["importance"]?.ToString()!) : null,
                    Status = jObject != null && jObject.ContainsKey("status")
                        ? Enum.Parse<Microsoft.Graph.Beta.Models.TaskStatus>(jObject?["status"]?.ToString()!) : null,
                    Body = new()
                    {
                        Content = jObject?["content"]?.ToString(),
                        ContentType = jObject != null && jObject.ContainsKey("bodyType") ? Enum.Parse<BodyType>(jObject?["bodyType"]?.ToString()!) : null
                    },
                    Categories = jObject != null && jObject.ContainsKey("categories") ? jObject?["categories"]?.ToString()?.Split(",").ToList() : null,
                };

                var result = await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()].Tasks.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteTodoTask")]
        [Description("Deletes a Todo task")]
        [Parameter(name: "taskListId", type: "string", required: true, visible: false, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, visible: false, description: "Id of the task")]
        public Task<string> DeleteTodoTask([ActionTurnContext] TurnContext turnContext,
                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var task = await graphClient.Me.Todo.Lists[parameters?["taskListId"]?.ToString()].Tasks[parameters?["taskId"]?.ToString()].GetAsync();

                    var title = task?.Title ?? string.Empty;
                    var body = task?.Body?.Content ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Title", type: "string", readOnly: true), title),
                        (new ParameterAttribute(name: "Content", type: "string", readOnly: true), body)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTodoTaskSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTodoTask", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()].Tasks[jObject?["taskId"]?.ToString()].DeleteAsync();

                    return "Task deleted";
                }, cancellationToken);
        }

    }
}

