using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsChatPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Chat")
    {

        [Action("MicrosoftGraph.ListChats")]
        [Description("List chats with Microsoft Graph")]
        public Task<string> ListChats([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Chats
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetChatMessages")]
        [Description("Gets chat messages by chat with Microsoft Graph")]
        [Parameter(name: "chatId", type: "string", required: true, description: "Id of the chat")]
        public Task<string> GetChatMessages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Chats[parameters["chatId"]?.ToString()].Messages
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Orderby = ["createdDateTime desc"];
                                });

                        return result?.Value;
                    });
        }
    }
}
