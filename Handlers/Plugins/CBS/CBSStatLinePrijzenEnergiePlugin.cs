using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLinePrijzenEnergiePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Prijzen Energie")
    {
        [Action("CBS.PrijzenEnergie.GetEindverbruikersprijzen")]
        [Description("Deze tabel toont de gemiddelde prijzen voor aardgas en elektriciteit. De totaalprijs is de som van de leveringsprijs en de netwerkprijs")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetEindverbruikersprijzen([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "85666NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.PrijzenEnergie.GetGemiddeldeEnergietarieven")]
        [Description("Deze tabel bevat cijfers over de consumentenprijzen van elektriciteit en gas. Deze zijn onderverdeeld in transportprijzen, leveringsprijzen en belastingen (in- en exclusief btw). De cijfers worden als gewogen gemiddelde maandprijzen gepubliceerd")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetGemiddeldeEnergietarieven([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "85592NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.PrijzenEnergie.GetPompprijzenBrandstoffenPerDag")]
        [Description("Deze tabel bevat pompprijzen van motorbrandstoffen. Er worden gewogen gemiddelde dagprijzen gepubliceerd van benzine Euro95, dieselolie en LPG inclusief BTW en accijns. Deze dagprijzen worden eens per week gepubliceerd")]
        [Parameter(name: "yearMonth", type: "string", required: true, description: "Year and month of the cijfers. In this format: YYYYMM")]
        public Task<string> GetPompprijzenBrandstoffenPerDag([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "80416ned", parameters["yearMonth"]?.ToString()!, parameters);
        }

    }
}
