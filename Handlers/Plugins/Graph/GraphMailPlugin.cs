using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta.Me.SendMail;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail")
    {

        [Action("MicrosoftGraph.ListMyMailMessages")]
        [Description("List my mail messages from Outlook with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> ListMyMailMessages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Messages
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                                    requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetMyMailFolders")]
        [Description("Gets my mail folders from Outlook with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> GetMyMailFolders([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.MailFolders
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                                    requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetMyMailMessagesByFolder")]
        [Description("Gets my mail messages from a folder with Microsoft Graph")]
        [Parameter(name: "folderId", type: "string", required: true, description: "Id of the folder")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> GetMyMailMessagesByFolder([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.MailFolders[parameters["folderId"].ToString()].Messages
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                                    requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ReplyToMailMessage")]
        [Description("Reply to a mail message")]
        [Parameter(name: "messageId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the message")]
        [Parameter(name: "toRecipients", type: "string", required: true, description: "Comma seperated list of mail addresses")]
        [Parameter(name: "comment", type: "string", required: true, multiline: true, description: "Comment to reply")]
        public Task<string> ReplyToMailMessage([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var messageId = parameters["messageId"]?.ToString();
                    var mail = await graphClient.Me.Messages[messageId].GetAsync();

                    var replyTo = mail?.Subject ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Reply-To", type: "string", readOnly: true), replyTo)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphReplyToMailMessageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.ReplyToMailMessage", data, async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var messageId = jObject?["messageId"]?.ToString();
                    var comment = jObject?["comment"]?.ToString();
                    var toRecipients = jObject?["toRecipients"]?.ToString().Split(",");

                    var requestBody = new Microsoft.Graph.Beta.Me.Messages.Item.Reply.ReplyPostRequestBody
                    {
                        Message = new Message
                        {
                            ToRecipients = toRecipients?.Select(t => t.ToRecipient()).ToList()
                        },
                        Comment = comment
                    };

                    await graphClient.Me.Messages[messageId].Reply.PostAsync(requestBody);

                    return "Reply-to send";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.SendMail")]
        [Description("Sends an email")]
        [Parameter(name: "toRecipients", type: "string", required: true, description: "Comma seperated list of to mail addresses")]
        [Parameter(name: "ccRecipients", type: "string", description: "Comma seperated list of cc mail addresses")]
        [Parameter(name: "subject", type: "string", required: true, description: "Subject of the mail")]
        [Parameter(name: "content", type: "string", required: true, multiline: true, description: "Content of the mail")]
        [Parameter(name: "contentType", type: "string", required: true, enumValues: ["Text", "Html"], description: "Content type of the mail")]
        public Task<string> SendMail([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphSendMailSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.SendMail", data, async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var subject = jObject?["subject"]?.ToString();
                    var content = jObject?["content"]?.ToString();
                    var contentType = jObject?["contentType"]?.ToString();
                    var toRecipients = jObject?["toRecipients"]?.ToString().Split(",");
                    var ccRecipients = jObject?["ccRecipients"]?.ToString().Split(",");

                    var requestBody = new SendMailPostRequestBody
                    {
                        SaveToSentItems = true,
                        Message = new Message
                        {
                            Subject = subject,
                            Body = new ItemBody
                            {
                                ContentType = Enum.Parse<BodyType>(contentType ?? Enum.GetName(BodyType.Text)!),
                                Content = content,
                            },
                            ToRecipients = toRecipients?.Select(t => t.ToRecipient()).ToList(),
                            CcRecipients = ccRecipients?.Select(t => t.ToRecipient()).ToList() ?? []
                        }
                    };

                    await graphClient.Me.SendMail.PostAsync(requestBody);

                    return "Mail send";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteMail")]
        [Description("Deletes a mail message")]
        [Parameter(name: "messageId", type: "string", required: true, visible: false, description: "Id of the message")]
        public Task<string> DeleteMail([ActionTurnContext] TurnContext turnContext,
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
        public Task MicrosoftGraphDeleteMailSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteMail", data, async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Messages[jObject?["messageId"]?.ToString()].DeleteAsync();

                    return "Mail deleted";
                }, cancellationToken);
        }


    }
}
