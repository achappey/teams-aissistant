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
using Newtonsoft.Json;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailDraftPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Drafts")
    {

        [Action("MicrosoftGraph.CreateDraftMailMessage")]
        [Description("Creates a new draft mail message")]
        [Parameter(name: "toRecipients", type: "string", required: true, description: "Comma seperated list of to mail addresses")]
        [Parameter(name: "ccRecipients", type: "string", description: "Comma seperated list of cc mail addresses")]
        [Parameter(name: "subject", type: "string", required: true, description: "Subject of the draft mail")]
        [Parameter(name: "content", type: "string", required: true, multiline: true, description: "Content of the draft mail")]
        [Parameter(name: "contentType", type: "string", required: true, enumValues: ["Text", "Html"], description: "Content type of the mail")]
        public Task<string> CreateDraftMailMessage([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateDraftMailMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateDraftMailMessage", data,
             async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var subject = jObject?["subject"]?.ToString();
                    var content = jObject?["content"]?.ToString();
                    var toRecipients = jObject?["toRecipients"]?.ToString().Split(",");
                    var ccRecipients = jObject?["ccRecipients"]?.ToString().Split(",");
                    var contentType = jObject?["contentType"]?.ToString();

                    var requestBody = new Message
                    {
                        Subject = subject,
                        Body = new()
                        {
                            ContentType = Enum.Parse<BodyType>(contentType ?? Enum.GetName(BodyType.Text)!),
                            Content = content,
                        },
                        ToRecipients = toRecipients?.Select(GraphExtensions.ToRecipient).ToList(),
                        CcRecipients = ccRecipients?.Select(GraphExtensions.ToRecipient).ToList()
                    };

                    var message = await graphClient.Me.Messages.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(message);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.CreateDraftReplyMessage")]
        [Description("Creates a new draft reply message")]
        [Parameter(name: "messageId", type: "string", required: true, visible: false, description: "Id of the message")]
        [Parameter(name: "comments", type: "string", required: true, multiline: true, description: "Content of the draft mail")]
        public Task<string> CreateDraftReplyMessage([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                  async (GraphServiceClient graphClient) =>
                  {
                      var messageId = parameters["messageId"]?.ToString();
                      var item = await graphClient.Me.Messages[messageId].GetAsync();

                      var subject = item?.Subject ?? string.Empty;
                      var bodyPreview = item?.BodyPreview ?? string.Empty;

                      return [
                          (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                        (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview)
                      ];
                  });
        }

        [Submit]
        public Task MicrosoftGraphCreateDraftReplyMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateDraftReplyMessage", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new Microsoft.Graph.Beta.Me.Messages.Item.CreateReply.CreateReplyPostRequestBody
                    {
                        Comment = jObject?["comments"]?.ToString(),
                    };

                    var message = await graphClient.Me.Messages[jObject?["messageId"]?.ToString()].CreateReply.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(message);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.CreateDraftReplyAllMessage")]
        [Description("Creates a new draft reply all message")]
        [Parameter(name: "messageId", type: "string", required: true, visible: false, description: "Id of the message")]
        [Parameter(name: "comments", type: "string", required: true, multiline: true, description: "Content of the draft mail")]
        public Task<string> CreateDraftReplyAllMessage([ActionTurnContext] TurnContext turnContext,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var messageId = parameters["messageId"]?.ToString();
                    var message = await graphClient.Me.Messages[messageId].GetAsync();

                    var subject = message?.Subject ?? string.Empty;
                    var bodyPreview = message?.BodyPreview ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                        (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateDraftReplyAllMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateDraftReplyAllMessage", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new Microsoft.Graph.Beta.Me.Messages.Item.CreateReplyAll.CreateReplyAllPostRequestBody
                    {
                        Comment = jObject?["comments"]?.ToString(),
                    };

                    var message = await graphClient.Me.Messages[jObject?["messageId"]?.ToString()].CreateReplyAll.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(message);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.CreateDraftForwardMessage")]
        [Description("Creates a new draft forward message")]
        [Parameter(name: "messageId", type: "string", required: true, visible: false, description: "Id of the message")]
        [Parameter(name: "toRecipients", type: "string", required: true, description: "Comma seperated list of to mail addresses")]
        [Parameter(name: "comments", type: "string", required: true, multiline: true, description: "Content of the draft mail")]
        public Task<string> CreateDraftForwardMessage([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var messageId = parameters["messageId"]?.ToString();
                    var item = await graphClient.Me.Messages[messageId].GetAsync();

                    var subject = item?.Subject ?? string.Empty;
                    var bodyPreview = item?.BodyPreview ?? string.Empty;

                    return [
                            (new ParameterAttribute(name: "Subject", type: "string", readOnly: true), subject),
                            (new ParameterAttribute(name: "BodyPreview", type: "string", readOnly: true), bodyPreview)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateDraftForwardMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateDraftForwardMessage", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var toRecipients = jObject?["toRecipients"]?.ToString().Split(",");

                var requestBody = new Microsoft.Graph.Beta.Me.Messages.Item.CreateForward.CreateForwardPostRequestBody
                {
                    Comment = jObject?["comments"]?.ToString(),
                    ToRecipients = toRecipients?.Select(GraphExtensions.ToRecipient).ToList(),
                };

                var message = await graphClient.Me.Messages[jObject?["messageId"]?.ToString()].CreateForward.PostAsync(requestBody);

                return JsonConvert.SerializeObject(message);
            }, cancellationToken);
        }
    }
}
