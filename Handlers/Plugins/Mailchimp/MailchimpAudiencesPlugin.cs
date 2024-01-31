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
    public class MailchimpAudiencesPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "Audiences")
    {

        [Action("Mailchimp.ListAudiences")]
        [Description("Lists audiences in Mailchimp")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "email", type: "string", description: "Restrict results to lists that include a specific subscriber's email address")]
        public Task<string> ListAudiences([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Lists.GetAllAsync(new()
                {
                    Offset = paramDict.Offset,
                    Email = parameters.TryGetValue("email", out object? value) ? value?.ToString() : null,
                }));
        }

        [Action("Mailchimp.GetListActivity")]
        [Description("Get up to the previous 180 days of daily detailed aggregated activity stats for a list, not including Automation activity")]
        [Parameter(name: "listId", type: "string", description: "Id of the list")]
        public Task<string> GetListActivity([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Activities.GetAllAsync(parameters["listId"]?.ToString()));
        }

        [Action("Mailchimp.GetListAbuseReports")]
        [Description("Get all abuse reports for a specific list")]
        [Parameter(name: "list_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> GetListAbuseReports([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.AbuseReports.GetAllAsync(parameters["list_id"]?.ToString()));
        }

        [Action("Mailchimp.GetListGrowthHistories")]
        [Description("Get a month-by-month summary of a specific list's growth activity")]
        [Parameter(name: "list_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> GetListGrowthHistories([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.GrowthHistories.GetAllAsync(parameters["list_id"]?.ToString()));
        }

        [Action("Mailchimp.AddList")]
        [Description("Create a new list in your Mailchimp account")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the list")]
        [Parameter(name: "company", type: "string", required: true, description: "The company name for the list")]
        [Parameter(name: "city", type: "string", required: true, description: "The city for the list contact")]
        [Parameter(name: "country", type: "string", required: true, description: "A two-character ISO3166 country code. Defaults to US if invalid")]
        [Parameter(name: "address1", type: "string", required: true, description: "The street address for the list contact")]
        [Parameter(name: "permission_reminder", type: "string", required: true, description: "The permission reminder for the list")]
        [Parameter(name: "email_type_option", type: "boolean", required: true, description: "Whether the list supports multiple formats for emails. When set to true, subscribers can choose whether they want to receive HTML or plain-text emails. When set to false, subscribers will receive HTML emails, with a plain-text alternative backup")]
        [Parameter(name: "from_name", type: "string", required: true, description: "The default from name for campaigns sent to this list")]
        [Parameter(name: "from_email", type: "string", required: true, description: "The default from email for campaigns sent to this list")]
        [Parameter(name: "subject", type: "string", required: true, description: "The default subject line for campaigns sent to this list")]
        [Parameter(name: "language", type: "string", required: true, description: "The default language for this lists's forms")]
        public Task<string> AddList([ActionTurnContext] TurnContext turnContext,
                                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MailchimpAddListSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "Mailchimp.AddList", data,
                async (MailChimpManager client, JObject? jObject) =>
                {
                    var result = await client.Lists.AddOrUpdateAsync(new()
                    {
                        Name = jObject?["name"]?.ToString(),
                        PermissionReminder = jObject?["permission_reminder"]?.ToString(),
                        Contact = new()
                        {
                            Address1 = jObject?["address1"]?.ToString(),
                            Company = jObject?["company"]?.ToString(),
                            City = jObject?["city"]?.ToString(),
                            Country = jObject?["country"]?.ToString()
                        },
                        EmailTypeOption = jObject?["email_type_option"]?.ToObject<bool>() ?? false,
                        CampaignDefaults = new()
                        {
                            FromEmail = jObject?["from_email"]?.ToString(),
                            FromName = jObject?["from_name"]?.ToString(),
                            Language = jObject?["language"]?.ToString(),
                            Subject = jObject?["subject"]?.ToString()
                        }
                    });

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("Mailchimp.GetListSegments")]
        [Description("Get information about all available segments for a specific list")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        public Task<string> GetListSegments([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.ListSegments.GetAllAsync(parameters["listId"]?.ToString()));
        }
    }
}
