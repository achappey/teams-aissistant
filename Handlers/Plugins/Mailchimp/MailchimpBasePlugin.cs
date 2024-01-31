using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json;
using MailChimp.Net;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Handlers.Plugins.Mailchimp.Models;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Handlers.Plugins.Mailchimp
{
    public abstract class MailchimpBasePlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name)
                : PluginBase(driveRepository, proactiveMessageService, name, "Mailchimp", "Marketing API", "v3")
    {
        protected readonly MailChimpManager _mailChimpManager = mailChimpManager;

        public async Task<string> ExecuteMailchimpQuery<T>(
                TurnContext turnContext, TeamsAIssistantState turnState, string actionName,
               Dictionary<string, object> parameters,
               Func<MailChimpManager, MailchimpQuery, Task<T>> query)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var queryParameters = parameters.ToMailchimpQuery();

            try
            {
                var result = await query(_mailChimpManager, queryParameters);
                var json = JsonConvert.SerializeObject(result);

                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, json, cardId);

                return json;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        protected async Task SubmitActionAsync(
          ITurnContext turnContext,
          TeamsAIssistantState turnState,
          string actionName,
          object data,
          Func<MailChimpManager, JObject?, Task<string>> actionMethod,
          CancellationToken cancellationToken)
        {
            JObject jObject = JObject.FromObject(data);
            var parametersDictionary = jObject?.ToObject<Dictionary<string, object>>();

            string result;

            try
            {
                result = await actionMethod(_mailChimpManager, jObject);
                await SendConfirmedCard(turnContext, actionName, parametersDictionary?.ExcludeVerb(), cancellationToken);
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            turnState.Temp.Input = turnContext.GetActionSubmitText(actionName, result);
        }

    }

}
