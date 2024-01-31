using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTodoTaskLinkedResourcesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Todo Task Linked Resources")
    {
        [Action("MicrosoftGraph.GetTodoTaskLinkedResources")]
        [Description("Gets a list of linked resources of a todo task")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, description: "Id of the task")]
        public Task<string> GetTodoTaskLinkedResources([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Todo.Lists[parameters["taskListId"]?.ToString()]
                            .Tasks[parameters["taskId"]?.ToString()]
                            .LinkedResources.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteTodoTaskLinkedResource")]
        [Description("Deletes a Todo task linked resource")]
        [Parameter(name: "taskListId", type: "string", required: true, visible: false, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, visible: false, description: "Id of the task")]
        [Parameter(name: "linkedResourceId", type: "string", required: true, visible: false, description: "Id of the linked resource")]
        public Task<string> DeleteTodoTaskLinkedResource([ActionTurnContext] TurnContext turnContext,
                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var taskListId = parameters?["taskListId"]?.ToString();
                    var taskId = parameters?["taskId"]?.ToString();

                    var task = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId].GetAsync();

                    var linkedResource = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId]
                        .LinkedResources[parameters?["linkedResourceId"]?.ToString()].GetAsync();

                    var name = linkedResource?.DisplayName ?? string.Empty;
                    var taskTitle = task?.Title ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Task", type: "string", readOnly: true), taskTitle),
                        (new ParameterAttribute(name: "Linked resource", type: "string", readOnly: true), name)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTodoTaskLinkedResourceSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTodoTaskLinkedResource", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()]
                    .Tasks[jObject?["taskId"]?.ToString()]
                    .LinkedResources[jObject?["linkedResourceId"]?.ToString()].DeleteAsync();

                    return "Linked resource deleted";
                }, cancellationToken);
        }

    }
}

