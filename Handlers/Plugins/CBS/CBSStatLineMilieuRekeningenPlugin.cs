using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineMilieuRekeningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Milieurekeningen")
    {
        [Action("CBS.Milieurekeningen.GetKerncijfersAfvalbalans")]
        [Description("Deze tabel bevat cijfers over de hoeveelheid afval die vrijkomt (herkomst) en waar het naar toe gaat (bestemming). Bij de herkomst wordt onderscheid gemaakt tussen herkomst uit de Nederlandse economie en het buitenland. Bij de bestemming van afval wordt onderscheid gemaakt naar verwerkingsmethoden in Nederland (hergebruik, verbranden en storten/lozen) en export")]
        public Task<string> GetKerncijfersBedrijven([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83555NED", "TypedDataSet", parameters);
        }

        [Action("CBS.Milieurekeningen.GetAfvalbalansHerkomstBestemming")]
        [Description("Deze tabel bevat alle HerkomstBestemmingen uit de Afvalbalans")]
        public Task<string> GetAfvalbalansHerkomstBestemming([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83555NED", "HerkomstBestemming", parameters);
        }

        [Action("CBS.Milieurekeningen.GetAardgasAardolieReserves")]
        [Description("De (fysieke) aardgas- en aardoliereserves bestaan uit de in Nederland aangetroffen hoeveelheden aardgas en aardolie die aangetoond en commercieel en sociaal-maatschappelijk winbaar zijn, vermeerderd met hoeveelheden waarvan winning in de toekomst aannemelijk is")]
        public Task<string> GetAardgasAardolieReserves([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "82539NED", "TypedDataSet", parameters);
        }

        [Action("CBS.Milieurekeningen.GetWaterGebruik")]
        [Description("Deze tabel bevat gegevens van de waterrekeningen, dit is een onderdeel van Milieurekeningen die het CBS jaarlijks samenstelt. In deze waterrekeningen is het (fysieke) gebruik van water door de Nederlandse economie opgenomen. Hierbij wordt onderscheid gemaakt tussen het gebruik van leidingwater, gebruik en de onttrekking van grondwater en van oppervlaktewater")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetWaterGebruik([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "82883NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.Milieurekeningen.GetWaterGebruikers")]
        [Description("Deze tabel bevat Watergebruikers tbv het WaterGebruik")]
        public Task<string> GetWaterGebruikers([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "82883NED", "Watergebruikers", parameters);
        }

    }
}
