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
    public class MailchimpReportsPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "Reports")
    {
        [Action("Mailchimp.ListCampaignReports")]
        [Description("Lists campaign reports in Mailchimp")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "before_send_time", type: "string", description: "Restrict the response to campaigns sent before the set time. Uses ISO 8601 time format: 2015-10-21T15:41:36+00:00")]
        [Parameter(name: "type", type: "string", enumValues: [ CampaignTypeConstants.Regular, CampaignTypeConstants.Plaintext,
            CampaignTypeConstants.Absplit, CampaignTypeConstants.Rss, CampaignTypeConstants.Variate ],
            description: "The campaign type")]
        public Task<string> ListCampaignReports([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Reports.GetAllReportsAsync(new MailChimp.Net.Core.ReportRequest()
                {
                    Offset = paramDict.Offset,
                    BeforeSendTime = parameters.TryGetValue("before_send_time", out object? value) ? DateTime.Parse(value?.ToString()!) : null,
                    Type = parameters.TryGetValue("type", out var statusValue)
                                && Enum.TryParse(statusValue?.ToString(), out MailChimp.Net.Core.CampaignType status)
                                    ? new List<MailChimp.Net.Core.CampaignType>() { status } : null,
                }));
        }

        [Action("Mailchimp.GetCampaignAdvice")]
        [Description("Get feedback based on a campaign's statistics. Advice feedback is based on campaign stats like opens, clicks, unsubscribes, bounces, and more")]
        [Parameter(name: "campaign_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> GetCampaignAdvice([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Reports.GetCampaignAdviceAsync(parameters["campaign_id"]?.ToString()));
        }

        [Action("Mailchimp.GetCampaignOpenReport")]
        [Description("Get detailed information about any campaign emails that were opened by a list member")]
        [Parameter(name: "campaign_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> GetCampaignOpenReport([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Reports.GetCampaignOpenReportAsync(parameters["campaign_id"]?.ToString()));
        }

        [Action("Mailchimp.GetClickReport")]
        [Description("Get information about clicks on specific links in your Mailchimp campaigns")]
        [Parameter(name: "campaign_id", type: "string", required: true, description: "The unique id for the campaign")]
        public Task<string> GetClickReport([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Reports.GetClickReportAsync(parameters["campaign_id"]?.ToString()));
        }
    }
}
