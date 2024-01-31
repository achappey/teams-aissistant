using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineEnergieBalansPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Energiebalans")
    {
        [Action("CBS.Energie.GetEnergiebalans")]
        [Description("Deze tabel bevat cijfers over het aanbod, de omzetting en het verbruik van energie. Energie komt onder andere vrij bij de verbranding van bijvoorbeeld aardgas, aardolie, steenkool en biobrandstoffen. Energie kan ook worden verkregen uit elektriciteit of warmte of worden onttrokken aan de natuur, bijvoorbeeld windkracht of zonne-energie. In de energiestatistiek heten al deze bronnen waaruit energie kan worden gebruikt 'energiedragers'")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "energiedragers", type: "string", required: true, description: "Key of the energiedragers")]
        public Task<string> GetKerncijfersBedrijven([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "83140NED", parameters["year"]?.ToString()!, parameters, $" and startswith(Energiedragers,'{parameters["energiedragers"]?.ToString()!}')");
        }

        [Action("CBS.Energie.GetEnergiedragers")]
        [Description("Deze tabel bevat energiedragers")]
        public Task<string> GetEnergiedragers([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83140NED", "Energiedragers", parameters);
        }

        [Action("CBS.Energie.GetEnergiebalansPerSector")]
        [Description("Deze tabel bevat cijfers over het aanbod, de omzetting en het verbruik van energie. Energie komt onder andere vrij bij de verbranding van bijvoorbeeld aardgas, aardolie, steenkool en biobrandstoffen. Energie kan ook worden verkregen uit elektriciteit of warmte of worden onttrokken aan de natuur, bijvoorbeeld windkracht of zonne-energie. In de energiestatistiek heten al deze bronnen waaruit energie kan worden gebruikt 'energiedragers'")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "sectoren", type: "string", required: true, description: "Key of the sectoren")]
        public Task<string> GetEnergiebalansPerSector([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "83989NED", parameters["year"]?.ToString()!, parameters, $" and startswith(Sectoren,'{parameters["sectoren"]?.ToString()!}')");
        }

        [Action("CBS.Energie.GetSectoren")]
        [Description("Deze tabel bevat sectoren")]
        public Task<string> GetSectoren([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83989NED", "Sectoren", parameters);
        }

    }
}
