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
    public class CBSStatLinePrijzenKoopwoningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Prijzen Koopwoningen")
    {
        [Action("CBS.PrijzenKoopwoningen.GetIndexBestaandeKoopwoningenNederland")]
        [Description("Deze tabel geeft de prijsontwikkelingen weer van de voorraad van bestaande koopwoningen. Ook worden het aantal transacties, de gemiddelde verkoopprijs en de totale waarde van de verkoopprijzen van de verkochte woningen gepubliceerd. De prijsindexcijfers over de bestaande koopwoningen zijn gebaseerd op een integrale registratie van verkooptransacties van woningen door het Kadaster en WOZ-waarden van alle woningen in Nederland")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetIndexBestaandeKoopwoningenNederland([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "83906NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.PrijzenKoopwoningen.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83913NED", "RegioSCodes", parameters);
        }

        [Action("CBS.PrijzenKoopwoningen.GetIndexBestaandeKoopwoningenRegio")]
        [Description("Deze tabel geeft de prijsontwikkelingen weer van de voorraad van bestaande koopwoningen per regio. Ook worden het aantal transacties, de gemiddelde verkoopprijs en de totale waarde van de verkoopprijzen van de verkochte woningen gepubliceerd. De prijsindexcijfers over de bestaande koopwoningen zijn gebaseerd op een integrale registratie van verkooptransacties van woningen door het Kadaster en WOZ-waarden van alle woningen in Nederland")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public Task<string> GetIndexBestaandeKoopwoningenRegio([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "83913NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.PrijzenKoopwoningen.GetNieuweEnBestaandeKoopwoningen")]
        [Description("Deze tabel geeft de prijsontwikkeling weer van woningen die zijn gekocht door huishoudens uitgesplitst naar nieuwe en bestaande koopwoningen. Ook wordt het aantal transacties, de gemiddelde verkoopprijs en de totale som van de verkoopprijzen van de verkochte woningen gepubliceerd.  prijsindex 2015=100")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetNieuweEnBestaandeKoopwoningen([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "84064NED", parameters["year"]?.ToString()!, parameters);
        }

    }
}
