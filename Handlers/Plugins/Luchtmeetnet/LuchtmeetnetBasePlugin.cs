using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public abstract class LuchtmeetnetBasePlugin(IHttpClientFactory clientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name) 
            : PluginBase(driveRepository, proactiveMessageService, name, "Luchtmeetnet", "Open data", "v1")
    {
        protected readonly HttpClient client = clientFactory.GetDefaultClient("https://api.luchtmeetnet.nl/open_api/", "Luchtmeetnet");

        protected async Task<string> GetList(ITurnContext turnContext, TeamsAIssistantState turnState,
            string actionName, Dictionary<string, object> parameters, string url)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var fullUrl = client.GetFullUrl(url, parameters);
            using var response = await client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                return response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var serializer = new JsonSerializer();
            var json = serializer.Deserialize<JObject>(jsonReader);
            var jsonData = json?["data"]?.ToString();

            if (json == null || string.IsNullOrEmpty(jsonData))
            {
                return "No data found";
            }

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, jsonData, cardId);

            return json.ToString(); 
        }


    }


    public static class OrderComponentsByConstants
    {
        public const string Order = "order";
        public const string Formula = "formula";
        public const string NameNl = "name_nl";
        public const string NameEn = "name_en";
    }

    public static class OrderStationsByConstants
    {
        public const string Location = "location";
        public const string Number = "number";
    }

    public static class OrderMeasurementsByConstants
    {
        public const string TimestampMeasured = "timestamp_measured";
        public const string Formula = "formula";
    }

    public static class OrderByDirectionConstants
    {
        public const string Asc = "asc";
        public const string Desc = "desc";
    }

    public static class FormulaConstants
    {
        public const string LKI = "LKI";
        public const string NO2 = "NO2";
        public const string PM10 = "PM10";
        public const string O3 = "O3";
    }
}
