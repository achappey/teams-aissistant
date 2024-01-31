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
    public class GraphTeamsChatTabsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Chat Tabs")
    {
        [Action("MicrosoftGraph.ListTeamsChatTabs")]
        [Description("Retrieve the list of tabs of a chat")]
        [Parameter(name: "chatId", type: "string", required: true, description: "Id of the chat")]
        public Task<string> ListTeamsChatTabs([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Chats[parameters["chatId"]?.ToString()].Tabs
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteTeamsChatTab")]
        [Description("Deletes a teams chat tab")]
        [Parameter(name: "chatId", type: "string", required: true, visible: false, description: "Id of the chat")]
        [Parameter(name: "tabId", type: "string", required: true, visible: false, description: "Id of the tab")]
        public Task<string> DeleteTeamsChatTab([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                   async (GraphServiceClient graphClient) =>
                   {
                       var chatId = parameters["chatId"]?.ToString();
                       var tabId = parameters["tabId"]?.ToString();

                       var team = await graphClient.Chats[chatId].GetAsync();
                       var tab = await graphClient.Chats[chatId].Tabs[tabId].GetAsync();

                       var chatMembers = team != null && team.Members != null ? string.Join(",", team.Members.Select(t => t.DisplayName)) : string.Empty;
                       var tabDisplayName = tab?.DisplayName ?? string.Empty;

                       return [
                        (new ParameterAttribute(name: "Chat", type: "string", readOnly: true), chatMembers),
                        (new ParameterAttribute(name: "Tab", type: "string", readOnly: true), tabDisplayName)
                       ];
                   });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTeamsChatTabSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTeamsChatTab", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Chats[jObject?["chatId"]?.ToString()]
                        .Tabs[jObject?["tabId"]?.ToString()].DeleteAsync();

                    return "Tab deleted";
                }, cancellationToken);
        }

    }
}

