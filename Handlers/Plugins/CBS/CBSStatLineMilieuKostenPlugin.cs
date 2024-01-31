using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineMilieuKostenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Milieukosten")
    {
        [Action("CBS.Milieukosten.GetKerncijfersBedrijven")]
        [Description("Deze tabel geeft kerncijfers over de milieu-investeringen en de milieulasten door bedrijven (met 10 of meer werknemers) in de bedrijfstakken delfstoffenwinning, industrie, energievoorziening en waterwinning. Het begrip 'milieu', zoals in deze tabel gebruikt, omvat het leefklimaat buiten het bedrijfsterrein met inbegrip van het bodemklimaat of de bodem onder dat terrein")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetKerncijfersBedrijven([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "82861NED", parameters["year"]?.ToString()!, parameters);
        }


    }
}
