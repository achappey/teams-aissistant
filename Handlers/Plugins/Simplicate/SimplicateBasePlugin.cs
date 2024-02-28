using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Handlers.Plugins.Simplicate.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public abstract class SimplicateBasePlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        GraphClientServiceProvider graphClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name) : PluginBase(driveRepository, proactiveMessageService, name, "Simplicate", "REST API", "v2")
    {

        protected readonly SimplicateClientServiceProvider _simplicateClientServiceProvider = simplicateClientServiceProvider;



        public async Task<string> SearchItems([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters, string url, string? orderBy = null, string? flattenListProperty = null)
        {
            if (!turnState.IsAuthenticated())
            {
                return "Not authenticated";
            }

            var missingParams = VerifyParameters(actionName, parameters);
            if (missingParams != null)
            {
                return missingParams;
            }

            var paramAttributes = GetActionParameters(actionName)?.ToList() ?? [];

            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var client = await _simplicateClientServiceProvider.GetAuthenticatedSimplicateClient(graphClientServiceProvider.AadObjectId!);
            string order = orderBy != null ? $"&sort={orderBy}" : string.Empty;
            using var result = await client.GetAsync($"{url}?{parameters.ToFilterString(paramAttributes)}{order}&metadata=count,limit,offset");

            if (!result.IsSuccessStatusCode)
            {
                return result.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage;
            }


            var json = await result.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(json);  
            var jsonData = jsonObject["data"];
            List<dynamic?> resultList;

            if (!string.IsNullOrEmpty(flattenListProperty) && jsonData != null)
            {
                resultList = jsonData.Children<JObject>()
                    .SelectMany(item =>
                    {
                        var property = item[flattenListProperty];
                        return property != null ? property.Children() : Enumerable.Empty<JToken>();
                    })
                    .Select(jToken => jToken.ToObject<dynamic>())
                    .ToList();

                jsonObject["data"] = JToken.FromObject(resultList);
            }
            else
            {
                if (jsonData != null)
                {
                    resultList = jsonData.Children<JObject>()
                        .Select(jToken => jToken.ToObject<dynamic>())
                        .ToList();
                }
                else
                {
                    resultList = [];
                }
            }

            var updatedJson = jsonObject.ToString();
            var resultString = jsonObject["data"]?.ToString();

            if (string.IsNullOrEmpty(updatedJson) || string.IsNullOrEmpty(resultString))
            {
                return "No data found";
            }

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultString, cardId);

            return updatedJson;
        }

        public async Task AddParameterPropertiesAsync(
            ITurnContext turnContext,
            Dictionary<string, object> properties,
            List<ParameterAttribute> parameters,
            string parameterKey,
            string crmEndpoint,
            string propertyName,
            string propertyValueName)
        {
            if (properties.ContainsKey(parameterKey))
            {
                var item = await GetSingleItem(turnContext, $"{crmEndpoint}/{properties[parameterKey]}");
                parameters.Add(new  (name: propertyName, readOnly: true, type: "string"));
                properties.Add(propertyName, item?[propertyValueName]?.ToString() ?? string.Empty);
            }
        }


        protected async Task<JToken?> GetSingleItem([ActionTurnContext] ITurnContext turnContext, string url)
        {
            var client = await _simplicateClientServiceProvider.GetAuthenticatedSimplicateClient(graphClientServiceProvider.AadObjectId!);
            var result = await client.GetAsync($"{url}");

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpRequestException(result.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage);
            }

            var json = await result.Content.ReadAsStringAsync();
            var data = (JObject.Parse(json)?["data"]) ?? throw new HttpRequestException(result.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage);

            return data;
        }

        private async Task SubmitActionAsync(
            ITurnContext turnContext,
            TeamsAIssistantState turnState,
            string actionName,
            object data,
            Func<HttpClient, string, HttpContent, Task<HttpResponseMessage>> httpRequestFunc,
            string url,
            CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            var actionParams = GetActionParameters(actionName);
            var parametersDictionary = jObject?.ToObject<Dictionary<string, object>>()?.ExcludeVerb();
            parametersDictionary = parametersDictionary?.ToDictionary(a => a.Key, h => h.GetFormValue(actionParams));
            var nestedJson = parametersDictionary?.ConvertToNestedJson();
            var graphClient = await _simplicateClientServiceProvider.GetAuthenticatedSimplicateClient(graphClientServiceProvider.AadObjectId!);
            string? result;

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(nestedJson), Encoding.UTF8, "application/json");
                var response = await httpRequestFunc(graphClient, url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errors = await response?.Content?.ReadAsStringAsync(cancellationToken)!;
                    
                    throw new HttpRequestException(errors ?? response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage);
                }

                result = await response?.Content?.ReadAsStringAsync(cancellationToken)!;
                
                await SendConfirmedCard(turnContext, actionName, parametersDictionary, cancellationToken);
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            turnState.Temp.Input = turnContext.GetActionSubmitText(actionName, result);
        }


        protected Task SubmitNewActionAsync(
            ITurnContext turnContext,
            TeamsAIssistantState turnState,
            string actionName,
            object data,
            string url,
            CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, actionName, data,
                (client, uri, content) => client.PostAsync(uri, content), url, cancellationToken);
        }


        protected Task SubmitUpdateActionAsync(
            ITurnContext turnContext,
            TeamsAIssistantState turnState,
            string actionName,
            object data,
            string url,
            CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);

            return SubmitActionAsync(turnContext, turnState, actionName, data,
                (client, uri, content) => client.PutAsync($"{uri}/{jObject["id"]}", content), url, cancellationToken);
        }
    }
}
