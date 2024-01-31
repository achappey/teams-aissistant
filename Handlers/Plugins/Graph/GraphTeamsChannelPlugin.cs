using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsChannelPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Channel")
    {

        [Action("MicrosoftGraph.GetTeamChannels")]
        [Description("Gets team channels with Microsoft Graph")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> GetTeamChannels([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Channels
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Orderby = ["displayName"];
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetTeamChannelFilesFolder")]
        [Description("Gets team channel files folder with Microsoft Graph")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, description: "Id of the teams channel")]
        public Task<string> GetTeamChannelFilesFolder([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Teams[parameters["teamsId"]?.ToString()].Channels[parameters["channelId"]?.ToString()]
                            .FilesFolder.GetAsync());
        }


        [Action("MicrosoftGraph.CreateTeamChannel")]
        [Description("Creates a new channel in a Microsoft Teams")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Name of the channel")]
        [Parameter(name: "description", type: "string", description: "Description of the channel")]
        [Parameter(name: "membershipType", type: "string", required: true, enumValues: ["standard", "private"], description: "Membership type of the channel")]
        public Task<string> CreateTeamChannel([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
               async (GraphServiceClient graphClient) =>
               {
                   var teamId = parameters["teamsId"]?.ToString();
                   var team = await graphClient.Teams[teamId].GetAsync();

                   var teamName = team?.DisplayName ?? string.Empty;

                   return [
                       (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName)
                   ];
               });
        }

        [Submit]
        public Task MicrosoftGraphCreateTeamChannelSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTeamChannel", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var teamsId = jObject?["teamsId"]?.ToString();
                    var name = jObject?["name"]?.ToString();
                    var description = jObject?["description"]?.ToString();
                    var membershipType = jObject?["membershipType"]?.ToString();

                    var requestBody = new Channel
                    {
                        DisplayName = name,
                        Description = description,
                        MembershipType = membershipType != null ? Enum.Parse<ChannelMembershipType>(membershipType) : ChannelMembershipType.Standard
                    };

                    await graphClient.Teams[teamsId].Channels.PostAsync(requestBody);

                    return "Channel created";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdateTeamChannel")]
        [Description("Updates a channel in Microsoft Teams")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, visible: false, description: "Id of the teams channel")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Name of the channel")]
        [Parameter(name: "description", type: "string", description: "Description of the channel")]
        public Task<string> UpdateTeamChannel([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamId = parameters["teamsId"]?.ToString();
                    var team = await graphClient.Teams[teamId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphUpdateTeamChannelSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateTeamChannel", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var teamsId = jObject?["teamsId"]?.ToString();
                    var channelId = jObject?["channelId"]?.ToString();
                    var name = jObject?["name"]?.ToString();
                    var description = jObject?["description"]?.ToString();

                    var requestBody = new Channel
                    {
                        DisplayName = name,
                        Description = description,
                    };

                    await graphClient.Teams[teamsId].Channels[channelId].PatchAsync(requestBody);

                    return "Teams Channel updated";
                }, cancellationToken);
        }
    }
}
