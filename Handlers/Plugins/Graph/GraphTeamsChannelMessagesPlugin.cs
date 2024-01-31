using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsChannelMessagesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Channel Messages")
    {
        [Action("MicrosoftGraph.ListTeamsChannelMessages")]
        [Description("Retrieve the list of messages in a channel of a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, description: "Id of the channel")]
        public Task<string> ListTeamsChannelMessages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Channels[parameters["channelId"]?.ToString()].Messages
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListTeamsChannelMessageReplies")]
        [Description("Retrieve the message replies of messages in a channel of a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, description: "Id of the channel")]
        [Parameter(name: "messageId", type: "string", required: true, description: "Id of the message")]
        public Task<string> ListTeamsChannelMessageReplies([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()]
                            .Channels[parameters["channelId"]?.ToString()].Messages[parameters["messageId"]?.ToString()].Replies
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.SendChannelMessage")]
        [Description("Creates a new message in a Teams channel")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, visible: false, description: "Id of the channel")]
        [Parameter(name: "content", type: "string", required: true, description: "Content of the message")]
        [Parameter(name: "contentType", type: "string", required: true, enumValues: ["Text", "Html"], description: "Content type of the message")]
        public Task<string> SendChannelMessage([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamsId = parameters["teamsId"]?.ToString();
                    var channelId = parameters["channelId"]?.ToString();

                    var team = await graphClient.Teams[teamsId].GetAsync();
                    var channel = await graphClient.Teams[teamsId].Channels[channelId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;
                    var channelName = channel?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "Channel", type: "string", readOnly: true), channelName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphSendChannelMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.SendChannelMessage", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var message = jObject?.ToChatMessage();

                    if (message != null)
                    {
                        var result = await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                            .Channels[jObject?["channelId"]?.ToString()].Messages.PostAsync(message);

                        return JsonConvert.SerializeObject(result);
                    }

                    return AIConstants.AIUnknownErrorMessage;
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.SetTeamsChannelMessageReaction")]
        [Description("Sets a reactions on a teams channel message")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, visible: false, description: "Id of the channel")]
        [Parameter(name: "messageId", type: "string", required: true, description: "Id of the message")]
        [Parameter(name: "reactionType", type: "string", required: true, enumValues: ["like", "angry", "surprised", "heart", "sad", "laugh"],
            description: "Type of the reaction")]
        public async Task<string> SetTeamsChannelMessageReaction([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return await SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamsId = parameters["teamsId"]?.ToString();
                    var channelId = parameters["channelId"]?.ToString();
                    var messageId = parameters["messageId"]?.ToString();

                    var team = await graphClient.Teams[teamsId].GetAsync();
                    var channel = await graphClient.Teams[teamsId].Channels[channelId].GetAsync();
                    var message = await graphClient.Teams[teamsId].Channels[channelId].Messages[messageId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;
                    var channelName = channel?.DisplayName ?? string.Empty;
                    var messageContent = message?.Body?.Content ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "Channel", type: "string", readOnly: true), channelName),
                        (new ParameterAttribute(name: "Content", type: "string", readOnly: true), messageContent)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphSetTeamsChannelMessageReactionSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.SetTeamsChannelMessageReaction", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                        .Channels[jObject?["channelId"]?.ToString()]
                        .Messages[jObject?["messageId"]?.ToString()]
                        .SetReaction.PostAsync(new()
                        {
                            ReactionType = jObject?["reactionType"]?.ToString()
                        });

                    return "Reaction set";
                }, cancellationToken);
        }


        [Action("MicrosoftGraph.SendChannelMessageReply")]
        [Description("Creates a new reply message in a Teams channel")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "channelId", type: "string", required: true, visible: false, description: "Id of the channel")]
        [Parameter(name: "messageId", type: "string", required: true, visible: false, description: "Id of the parent message")]
        [Parameter(name: "content", type: "string", required: true, description: "Content of the reply message")]
        [Parameter(name: "contentType", type: "string", required: true, enumValues: ["Text", "Html"], description: "Content type of the message")]
        public Task<string> SendChannelMessageReply([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
            async (GraphServiceClient graphClient) =>
            {
                var teamsId = parameters["teamsId"]?.ToString();
                var channelId = parameters["channelId"]?.ToString();
                var messageId = parameters["messageId"]?.ToString();

                var team = await graphClient.Teams[teamsId].GetAsync();
                var channel = await graphClient.Teams[teamsId].Channels[channelId].GetAsync();
                var message = await graphClient.Teams[teamsId].Channels[channelId].Messages[messageId].GetAsync();

                var teamName = team?.DisplayName ?? string.Empty;
                var channelName = channel?.DisplayName ?? string.Empty;
                var messageContent = message?.Body?.Content ?? string.Empty;

                return [
                    (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                    (new ParameterAttribute(name: "Channel", type: "string", readOnly: true), channelName),
                    (new ParameterAttribute(name: "Message", type: "string", readOnly: true), messageContent)
                ];
            });
        }

        [Submit]
        public Task MicrosoftGraphSendChannelMessageReplySubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.SendChannelMessageReply", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var message = jObject?.ToChatMessage();

                    if (message != null)
                    {
                        var result = await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                            .Channels[jObject?["channelId"]?.ToString()]
                            .Messages[jObject?["messageId"]?.ToString()].Replies.PostAsync(message);

                        return JsonConvert.SerializeObject(result);
                    }

                    return AIConstants.AIUnknownErrorMessage;
                }, cancellationToken);
        }
    }
}

