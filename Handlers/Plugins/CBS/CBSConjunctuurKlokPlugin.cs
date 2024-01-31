using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Handlers.Plugins.CBS.Models;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSConjunctuurKlokPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : PluginBase(driveRepository, proactiveMessageService, "Conjunctuurklok", "CBS", "Dashboard-economie", "v1")
    {
        private readonly HttpClient client = httpClientFactory.GetDefaultClient($"https://www.cbs.nl/nl-nl/visualisaties/dashboard-economie/conjunctuur/conjunctuur", "Conjunctuurklok");

        [Action("CBS.GetConjunctuurKlok")]
        [Description("De Conjunctuurklok is een hulpmiddel voor het bepalen van de stand en het verloop van de Nederlandse conjunctuur. In de Conjunctuurklok komt vrijwel alle belangrijke economische informatie samen die het CBS tijdens de afgelopen maand c.q. het afgelopen kwartaal heeft gepubliceerd. Een belangrijk kenmerk van de conjunctuur is dat deze cyclisch verloopt. Perioden van hoge groei wisselen af met perioden van nauwelijks groei, of zelfs van krimp")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the conjunctuurklok")]
        public async Task<string> GetConjunctuurKlok([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            using var response = await client.GetAsync("/-/media/cbs/infographics/Dashboard-economie/data/conjunctuurklok-data.json");
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ConjunctuurKlok>>();

            var items = result?.Where(r => r.Maand!.Any(t => t.Contains(parameters?["year"]?.ToString()!)))
                .Select(a => new
                {
                    Month = a.Maand?.FirstOrDefault(),
                    Bbp = a.Bbp?.ExtractFirstData(),
                    Consumentenvertrouwen = a.Consumentenvertrouwen?.ExtractFirstData(),
                    Consumptie = a.Consumptie?.ExtractFirstData(),
                    Faillissementen = a.Faillissementen?.ExtractFirstData(),
                    GewerkteUren = a.Gewerkteuren?.ExtractFirstData(),
                    Investeringen = a.Investeringen?.ExtractFirstData(),
                    OmzetUitzendbranche = a.OmzetUitzendbranche?.ExtractFirstData(),
                    PrijzenKoopwoningen = a.PrijzenKoopwoningen?.ExtractFirstData(),
                    Producentenvertrouwen = a.Producentenvertrouwen?.ExtractFirstData(),
                    Productie = a.Productie?.ExtractFirstData(),
                    Uitvoer = a.Uitvoer?.ExtractFirstData(),
                    Vacatures = a.Vacatures?.ExtractFirstData(),
                    Werkloosheid = a.Werkloosheid?.ExtractFirstData()
                });

            var cleanJson = System.Text.Json.JsonSerializer.Serialize(items);

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, cleanJson, cardId);
            return cleanJson;
        }

    }
}
