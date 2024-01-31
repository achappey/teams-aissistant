using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineVervoermiddelenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Vervoermiddelen")
    {
        [Action("CBS.Vervoermiddelen.GetEmissiesNaarLucht")]
        [Description("Deze tabel bevat cijfers over feitelijke totale emissies naar lucht op Nederlands grondgebied door het totale wegverkeer, inclusief buitenlandse voertuigen. Daarnaast bevat deze tabel cijfers over de bijbehorende parkemissiefactoren, dit zijn de gemiddelde emissies per voertuigkilometer")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetEmissiesNaarLucht([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "85347NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.Vervoermiddelen.GetVoertuigtypes")]
        [Description("Deze tabel bevat voertuigtypes")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetVoertuigtypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "85347NED", "Voertuigtype", parameters);
        }


    }
}
