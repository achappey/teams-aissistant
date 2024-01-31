using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using MailChimp.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Mailchimp
{
    public class MailchimpFileManagerPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "File Manager")
    {
        [Action("Mailchimp.ListFiles")]
        [Description("Get a list of available images and files stored in the File Manager for the account")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "since_created_at", type: "string", description: "Restrict the response to files created after the set date. Uses ISO 8601 time format: 2015-10-21T15:41:36+00:00")]
        [Parameter(name: "before_created_at", type: "string", description: "Restrict the response to files created before the set date")]
        public Task<string> ListFiles([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.FileManagerFiles.GetAllAsync(new ()
                {
                    Offset = paramDict.Offset,
                    SinceCreatedAt = parameters.TryGetValue("since_created_at", out object? since_date_created)
                                ? DateTime.Parse(since_date_created.ToString()!) : null,
                    BeforeCreatedAt = parameters.TryGetValue("before_created_at", out object? before_date_created)
                                ? DateTime.Parse(before_date_created.ToString()!) : null
                }));
        }

        [Action("Mailchimp.ListFolders")]
        [Description("Get a list of all folders in the File Manager")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "since_created_at", type: "string", description: "Restrict the response to folders created after the set date. Uses ISO 8601 time format: 2015-10-21T15:41:36+00:00")]
        [Parameter(name: "before_created_at", type: "string", description: "Restrict the response to folders created before the set date")]
        public Task<string> ListFolders([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.FileManagerFolders.GetAllAsync(new ()
                {
                    Offset = paramDict.Offset,
                    SinceCreatedAt = parameters.TryGetValue("since_created_at", out object? since_date_created)
                                ? DateTime.Parse(since_date_created.ToString()!) : null,
                    BeforeCreatedAt = parameters.TryGetValue("before_created_at", out object? before_date_created)
                                ? DateTime.Parse(before_date_created.ToString()!) : null
                }));
        }

        [Action("Mailchimp.CreateFileFolder")]
        [Description("Creates a new Mailchimp file folder")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the file folder")]
        public Task<string> CreateFileFolder([ActionTurnContext] TurnContext turnContext,
                                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MailchimpCreateFileFolderSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "Mailchimp.CreateFileFolder", data,
                async (MailChimpManager client, JObject? jObject) =>
                {
                    var name = jObject?["name"]?.ToString();
                    var result = await client.FileManagerFolders.AddAsync(name: name);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }
    }
}
