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
    public class GraphTeamsChannelTabsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Channel Tabs")
    {
        [Action("MicrosoftGraph.ListTeamsChannelTabs")]
        [Description("Retrieve the list of tabs of a channel")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, description: "Id of the channel")]
        public Task<string> ListTeamsChannelTabs([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()]
                            .Channels[parameters["channelId"]?.ToString()].Tabs
                            .GetAsync();

                        return result?.Value;
                    });
        }


        [Action("MicrosoftGraph.DeleteTeamsChannelTab")]
        [Description("Deletes a teams channel tab")]
        [Parameter(name: "teamId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, visible: false, description: "Id of the channel")]
        [Parameter(name: "tabId", type: "string", required: true, visible: false, description: "Id of the tab")]
        public Task<string> DeleteTeamsChannelTab([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphServiceClient) =>
                {
                    var teamId = parameters["teamId"]?.ToString();
                    var channelId = parameters["channelId"]?.ToString();
                    var team = await graphServiceClient.Teams[teamId].GetAsync();
                    var channel = await graphServiceClient.Teams[teamId].Channels[channelId].GetAsync();
                    var tab = await graphServiceClient.Teams[teamId].Channels[channelId].Tabs[parameters["tabId"]?.ToString()].GetAsync();
                   
                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), team?.DisplayName  ?? string.Empty),
                        (new ParameterAttribute(name: "Channel", type: "string", readOnly: true), channel?.DisplayName  ?? string.Empty),
                        (new ParameterAttribute(name: "Tab", type: "string", readOnly: true), tab?.DisplayName  ?? string.Empty)
                     ];
                });
        }
    
        [Submit]
        public Task MicrosoftGraphDeleteTeamsChannelTabSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTeamsChannelTab", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Teams[jObject?["teamId"]?.ToString()].Channels[jObject?["channelId"]?.ToString()]
                        .Tabs[jObject?["tabId"]?.ToString()].DeleteAsync();

                    return "Tab deleted";
                }, cancellationToken);
        }
    }
}


