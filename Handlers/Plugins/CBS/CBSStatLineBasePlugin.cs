using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public abstract class CBSStatLineBasePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory, string name)
        : PluginBase(driveRepository, proactiveMessageService, name, "CBS", "StatLine", "v1")
    {
        private readonly HttpClient client = httpClientFactory.GetDefaultClient($"https://opendata.cbs.nl/ODataApi/odata/", "StatLine");

        public async Task<string> ExecuteStatLineQuery(
            ITurnContext turnContext, TeamsAIssistantState turnState, string actionName, string endpoint, string year,
           Dictionary<string, object> parameters, string? otherQueries = null)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var missingParams = VerifyParameters(actionName, parameters);

            if (missingParams != null)
            {
                return missingParams;
            }

            try
            {
                using var response = await client.GetAsync($"{endpoint}/TypedDataSet?$filter=startswith(Perioden,'{year}'){otherQueries ?? string.Empty}");
                var resultString = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(resultString)?.GetValue("value")?.ToString();

                if (jObject == null)
                {
                    return AIConstants.AIUnknownErrorMessage;
                }

                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, jObject, cardId);

                return jObject;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> ExecuteStatLineBaseQuery(
            ITurnContext turnContext, TeamsAIssistantState turnState, string actionName, string endpoint, string baseEndpoint,
           Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var missingParams = VerifyParameters(actionName, parameters);

            if (missingParams != null)
            {
                return missingParams;
            }

            try
            {
                using var response = await client.GetAsync($"{endpoint}/{baseEndpoint}");
                var resultString = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(resultString)?.GetValue("value")?.ToString();

                if (jObject == null)
                {
                    return AIConstants.AIUnknownErrorMessage;
                }

                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, jObject, cardId);

                return jObject;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
