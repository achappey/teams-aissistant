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
    public class GraphTodoTaskAttachmentsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Todo Task Attachments")
    {
        [Action("MicrosoftGraph.GetTodoTaskAttachments")]
        [Description("Gets a list of attachments of a todo task")]
        [Parameter(name: "taskListId", type: "string", required: true, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, description: "Id of the task")]
        public Task<string> GetTodoTaskAttachments([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Todo.Lists[parameters["taskListId"]?.ToString()]
                            .Tasks[parameters["taskId"]?.ToString()]
                            .Attachments.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteTodoTaskAttachment")]
        [Description("Deletes a Todo task attachment")]
        [Parameter(name: "taskListId", type: "string", required: true, visible: false, description: "Id of the task list")]
        [Parameter(name: "taskId", type: "string", required: true, visible: false, description: "Id of the task")]
        [Parameter(name: "attachmentId", type: "string", required: true, visible: false, description: "Id of the attachment")]
        public Task<string> DeleteTodoTaskAttachment([ActionTurnContext] TurnContext turnContext,
                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var taskListId = parameters?["taskListId"]?.ToString();
                    var taskId = parameters?["taskId"]?.ToString();

                    var task = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId].GetAsync();

                    var attachment = await graphClient.Me.Todo.Lists[taskListId]
                        .Tasks[taskId]
                        .Attachments[parameters?["attachmentId"]?.ToString()].GetAsync();

                    var name = attachment?.Name ?? string.Empty;
                    var taskTitle = task?.Title ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Task", type: "string", readOnly: true), taskTitle),
                        (new ParameterAttribute(name: "Attachment", type: "string", readOnly: true), name)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTodoTaskAttachmentSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTodoTaskAttachment", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Todo.Lists[jObject?["taskListId"]?.ToString()].Tasks[jObject?["taskId"]?.ToString()].Attachments[jObject?["attachmentId"]?.ToString()].DeleteAsync();

                    return "Attachment deleted";
                }, cancellationToken);
        }

    }
}

