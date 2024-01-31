using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailAttachmentsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Attachments")
    {
        [Action("MicrosoftGraph.ListMessageAttachments")]
        [Description("List attachments of a mail message with Microsoft Graph")]
        [Parameter(name: "messageId", type: "string", required: true, description: "Id of the message")]
        public Task<string> ListMessageAttachments([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Messages[parameters["messageId"]?.ToString()].Attachments
                            .GetAsync();

                        return result?.Value;
                    });
        }
    }
}
