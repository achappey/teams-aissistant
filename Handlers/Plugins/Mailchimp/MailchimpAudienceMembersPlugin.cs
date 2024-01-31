using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using MailChimp.Net;
using MailChimp.Net.Models;

namespace TeamsAIssistant.Handlers.Plugins.Mailchimp
{
    public class MailchimpAudienceMembersPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "Audience Members")
    {

        [Action("Mailchimp.GetListMembers")]
        [Description("Get information about members in a specific Mailchimp list")]
        [Parameter(name: "listId", type: "string", description: "Id of the list")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "status", type: "string", enumValues: ["subscribed", "unsubscribed", "cleaned", "pending", "transactional", "archived"],
            description: "The subscriber's status")]
        [Parameter(name: "since_timestamp_opt", type: "string", description: "Restrict results to subscribers who opted-in after the set timeframe. Uses ISO 8601 time format: 2015-10-21T15:41:36+00:00")]
        [Parameter(name: "before_timestamp_opt", type: "string", description: "Restrict results to subscribers who opted-in before the set timeframe")]
        public Task<string> GetListMembers([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Members.GetAllAsync(parameters["listId"]?.ToString(), new ()
                {
                    Offset = paramDict.Offset,
                    SinceTimestamp = parameters["since_timestamp_opt"]?.ToString(),
                    BeforeTimestamp = parameters["since_timestamp_opt"]?.ToString(),
                    Status = parameters.TryGetValue("status", out object? status) == true ? Enum.Parse<Status>(status?.ToString()!) : null,
                }));
        }

        [Action("Mailchimp.ListMemberTags")]
        [Description("Get the tags on a list member")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        [Parameter(name: "subscriber_hash", type: "string", required: true, description: "The MD5 hash of the lowercase version of the list member's email address. This endpoint also accepts a list member's email address or contact_id")]
        public Task<string> ListMemberTags([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
               [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Members.GetTagsAsync(parameters["listId"]?.ToString(), parameters["subscriber_hash"]?.ToString()));
        }

        [Action("Mailchimp.ListMemberActivities")]
        [Description("Get the last 50 events of a member's activity on a specific list, including opens, clicks, and unsubscribes")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        [Parameter(name: "subscriber_hash", type: "string", required: true, description: "The MD5 hash of the lowercase version of the list member's email address. This endpoint also accepts a list member's email address or contact_id")]
        public Task<string> ListMemberActivities([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Members.GetActivitiesAsync(parameters["listId"]?.ToString(), parameters["subscriber_hash"]?.ToString()));
        }

        [Action("Mailchimp.ListMemberNotes")]
        [Description("Get recent notes for a specific list member")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        [Parameter(name: "subscriber_hash", type: "string", required: true, description: "The MD5 hash of the lowercase version of the list member's email address")]
        public Task<string> ListMemberNotes([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Notes.GetAllAsync(parameters["listId"]?.ToString(), parameters["subscriber_hash"]?.ToString()));
        }
    }
}
