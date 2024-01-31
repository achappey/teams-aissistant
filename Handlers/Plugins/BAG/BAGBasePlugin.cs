using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Config;
using TeamsAIssistant.Extensions;
using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using Newtonsoft.Json.Linq;
using Microsoft.Teams.AI.AI.Action;
using Newtonsoft.Json;
using TeamsAIssistant.Attributes;
using System.ComponentModel;
using TeamsAIssistant.Handlers.Plugins.BAG.Models;

namespace TeamsAIssistant.Handlers.Plugins.BAG
{
    public class BAGBasePlugin : PluginBase
    {
        protected readonly HttpClient client;

        public BAGBasePlugin(IHttpClientFactory clientFactory, IConfiguration configuration,
                ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : base(driveRepository, proactiveMessageService, "Basisregistratie Adressen en Gebouwen", "Kadaster", "Basisregistratie Adressen en Gebouwen", "v2")
        {
            client = clientFactory.GetDefaultClient("https://api.bag.kadaster.nl/lvbag/individuelebevragingen/v2/", "BAG");

            var bagApiKey = configuration.Get<ConfigOptions>()?.BAGApiKey;
            client.DefaultRequestHeaders.Add("X-Api-Key", bagApiKey);
            client.DefaultRequestHeaders.Add("Accept-Crs", "epsg:28992");
        }

        public async Task<string?> ExecuteBagQuery(
                     ITurnContext turnContext, TeamsAIssistantState turnState, string actionName, string endpointUrl,
                    Dictionary<string, object> parameters,
                    Func<JObject?, string?> query)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var queryString = parameters?.BuildQueryString();

            try
            {
                using var response = await client.GetAsync($"{endpointUrl}?{queryString}");
                var stringData = await response.Content.ReadAsStringAsync();
                var embedded = JObject.Parse(stringData)["_embedded"]?.ToString();

                if (!string.IsNullOrEmpty(embedded))
                {
                    var embeddedObject = JObject.Parse(embedded);
                    var resultData = query(embeddedObject);

                    if (!string.IsNullOrEmpty(resultData))
                    {
                        await UpdateFunctionCard(turnContext, turnState, actionName, parameters ?? [], resultData, cardId);

                        return resultData;
                    }

                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return "No data found";
        }

        [Action("BAG.SearchAdressenUitgebreid")]
        [Description("Search for detailed BAG address information by query")]
        [Parameter(name: "q", type: "string", required: true, description: "The search query")]
        [Parameter(name: "page", type: "number", description: "The page number")]
        [Parameter(name: "pageSize", type: "number", minimum: 10, maximum: 100, description: "The page size. Defaults to 20")]
        public Task<string?> SearchAdressenUitgebreid([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteBagQuery(
              turnContext, turnState, actionName, "adressenuitgebreid", parameters,
              (result) =>
                  {
                      var adresses = result?["adressen"]?.ToObject<IEnumerable<Adres>>()?.WithLatLong();
                      return JsonConvert.SerializeObject(adresses);
                  });
        }

        [Action("BAG.SearchOpenbareRuimten")]
        [Description("Search for BAG openbare ruimten information")]
        [Parameter(name: "woonplaatsIdentificatie", required: true, type: "string", description: "The woonplaats identifier")]
        [Parameter(name: "page", type: "number", description: "The page number")]
        [Parameter(name: "pageSize", type: "number", minimum: 10, maximum: 100, description: "The page size. Defaults to 20")]
        public Task<string?> SearchOpenbareRuimten([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteBagQuery(
              turnContext, turnState, actionName, "openbareruimten", parameters,
              (result) =>
                  {
                      return result?["openbareruimten"]?.ToString();
                  });
        }

        [Action("BAG.SearchWoonplaatsen")]
        [Description("Search for BAG woonplaats information")]
        [Parameter(name: "naam", type: "string", description: "The name of the woonplaats")]
        [Parameter(name: "page", type: "number", description: "The page number")]
        [Parameter(name: "pageSize", type: "number", minimum: 10, maximum: 100, description: "The page size. Defaults to 20")]
        public Task<string?> SearchWoonplaatsen([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteBagQuery(
              turnContext, turnState, actionName, "woonplaatsen", parameters,
              (result) =>
                  {
                      return result?["woonplaatsen"]?.ToString();
                  });
        }

    }
}
