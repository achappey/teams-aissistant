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
    public class GraphTodoTaskChecklistPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Todo Task Checklist")
    {
        [Action("MicrosoftGraph.GetTodoTaskChecklistItems")]
        [Description("Gets a list of checklist items of a todo task")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, description: "Id of the task")]
        public Task<string> GetTodoTaskChecklist([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Todo.Lists[parameters["taskListId"]?.ToString()]
                            .Tasks[parameters["taskId"]?.ToString()]
                            .ChecklistItems.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateTodoTaskChecklistItem")]
        [Description("Creates a new Todo task checlist item")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, description: "Id of the task")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Name of the checklist item")]
        [Parameter(name: "isChecked", type: "boolean", description: "Is checked")]
        public Task<string> CreateTodoTaskChecklistItem([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateTodoTaskChecklistItemSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTodoTaskChecklistItem", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new ChecklistItem
                {
                    DisplayName = jObject?["displayName"]?.ToString(),
                    IsChecked = jObject != null && jObject.ContainsKey("isChecked") 
                        ? jObject?["isChecked"]?.ToObject<bool>() : null,
                };

                var checklistItem = await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()]
                        .Tasks[jObject?["taskId"]?.ToString()].ChecklistItems.PostAsync(requestBody);

                return JsonConvert.SerializeObject(checklistItem);
            }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteTodoTaskChecklistItem")]
        [Description("Deletes a Todo task checklist item")]
        [Parameter(name: "taskListId", type: "string", required: true, visible: false, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, visible: false, description: "Id of the task")]
        [Parameter(name: "checklistItemId", type: "string", required: true, visible: false, description: "Id of the checklist item")]
        public Task<string> DeleteTodoTaskChecklistItem([ActionTurnContext] TurnContext turnContext,
                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var taskListId = parameters?["taskListId"]?.ToString();
                    var taskId = parameters?["taskId"]?.ToString();

                    var task = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId].GetAsync();

                    var checklistItem = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId]
                        .ChecklistItems[parameters?["checklistItemId"]?.ToString()].GetAsync();

                    var name = checklistItem?.DisplayName ?? string.Empty;
                    var taskTitle = task?.Title ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Task", type: "string", readOnly: true), taskTitle),
                        (new ParameterAttribute(name: "Checklist item", type: "string", readOnly: true), name)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTodoTaskChecklistItemSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTodoTaskChecklistItem", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()]
                    .Tasks[jObject?["taskId"]?.ToString()]
                    .ChecklistItems[jObject?["checklistItemId"]?.ToString()].DeleteAsync();

                    return "Checklist item deleted";
                }, cancellationToken);
        }

    }
}

