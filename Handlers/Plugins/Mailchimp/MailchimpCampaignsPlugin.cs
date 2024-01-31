using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using MailChimp.Net;
using TeamsAIssistant.Handlers.Plugins.Mailchimp.Models;

namespace TeamsAIssistant.Handlers.Plugins.Mailchimp
{
    public class MailchimpCampaignsPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "Campaigns")
    {
        [Action("Mailchimp.ListCampaigns")]
        [Description("Lists campaigns in Mailchimp")]
        [Parameter(name: "status", type: "string", enumValues: [CampaignStatusConstants.Save, CampaignStatusConstants.Paused,
            CampaignStatusConstants.Schedule, CampaignStatusConstants.Sending, CampaignStatusConstants.Sent], description: "The status of the campaign")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        public Task<string> ListCampaigns([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Campaigns.GetAllAsync(new()
                {
                    Offset = paramDict.Offset,
                    Status = parameters.TryGetValue("status", out var statusValue)
                                && Enum.TryParse(statusValue?.ToString(), out MailChimp.Net.Core.CampaignStatus status) ? status : null,
                }));
        }

        [Action("Mailchimp.ListCampaignFeedback")]
        [Description("Get feedback based on a campaign's statistics. Advice feedback is based on campaign stats like opens, clicks, unsubscribes, bounces, and more.")]
        [Parameter(name: "campaign_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> ListCampaignFeedback([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Feedback.GetAllAsync(parameters["campaign_id"]?.ToString()));
        }

      


    }
}
