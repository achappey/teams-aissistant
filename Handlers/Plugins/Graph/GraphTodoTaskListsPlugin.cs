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
    public class GraphTodoTaskListsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Todo Task Lists")
    {

        [Action("MicrosoftGraph.GetMyTodoTaskLists")]
        [Description("Get a list of the todoTaskList objects and their properties")]
        public Task<string> GetMyTodoTaskLists([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Todo.Lists
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateTodoTaskList")]
        [Description("Creates a new Todo task list")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Name of the task list")]
        public Task<string> CreateTodoTaskList([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateTodoTaskListSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTodoTaskList", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new TodoTaskList
                {
                    DisplayName = jObject?["displayName"]?.ToString(),
                };

                var result = await graphClient.Me.Todo.Lists.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteTodoTaskList")]
        [Description("Deletes a Todo task list")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        public Task<string> DeleteTodoTaskList([ActionTurnContext] TurnContext turnContext,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var taskListId = parameters["taskListId"]?.ToString();
                    var list = await graphClient.Me.Todo.Lists[taskListId].GetAsync();
                    var name = list?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Name", type: "string", readOnly: true), name)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTodoTaskListSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTodoTaskList", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new TodoTaskList
                {
                    DisplayName = jObject?["displayName"]?.ToString(),
                };

                var result = await graphClient.Me.Todo.Lists.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }


    }
}
