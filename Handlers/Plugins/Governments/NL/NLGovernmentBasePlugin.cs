using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using System.Text;
using TeamsAIssistant.Constants;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public abstract class NLGovernmentBasePlugin(TeamsAdapter teamsAdapter,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name) 
        : PluginBase(driveRepository, proactiveMessageService, name, "Rijksoverheid", "Open data", "v1")
    {
        protected readonly HttpClient client = teamsAdapter.GetDefaultClient("https://opendata.rijksoverheid.nl/v1/", "Rijksoverheid");

        protected Task<string> GetNLGovernmentList(ITurnContext turnContext, TeamsAIssistantState turnState,
              string actionName, Dictionary<string, object> parameters, string url, IEnumerable<string>? excludeProps = null)
        {
            return FetchDataFromApi(turnContext, turnState, actionName, parameters, () =>
            {
                var urlBuilder = new StringBuilder($"{url}?output=json&");
                urlBuilder.Append(parameters.ToFilterString(excludeProps));
                return urlBuilder.ToString();
            });
        }

        protected Task<string> GetNLGovernmentItem(ITurnContext turnContext, TeamsAIssistantState turnState,
            string actionName, Dictionary<string, object> parameters, string url)
        {
            return FetchDataFromApi(turnContext, turnState, actionName, parameters, () =>
                $"{url}/{parameters["id"]}?output=json");
        }

        private async Task<string> FetchDataFromApi(ITurnContext turnContext, TeamsAIssistantState turnState,
            string actionName, Dictionary<string, object> parameters, Func<string> buildUrlFunc)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            using var response = await client.GetAsync(buildUrlFunc());

            if (!response.IsSuccessStatusCode)
            {
                return response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage;
            }

            var result = await response.Content.ReadAsStringAsync();
            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, result, cardId);

            return result;
        }
    }
}
