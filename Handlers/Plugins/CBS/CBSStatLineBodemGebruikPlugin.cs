using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineBodemGebruikPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Bodem Gebruik")
    {
        [Action("CBS.BodemGebruik.GetPerProvincie")]
        [Description("Deze tabel heeft als doel inzicht te verschaffen in het gebruik van de beschikbare ruimte van Nederland en in de veranderingen die zich daarin voordoen. Het Bestand Bodemgebruik (BBG) ligt ten grondslag aan deze tabel. Voor tussenliggende peiljaren waarvoor geen Bestand Bodemgebruik beschikbaar is, worden uitsluitend de totale oppervlaktes van de gepresenteerde regio’s opgenomen.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetPerProvincie([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "37105", parameters["year"]?.ToString()!, parameters);
        }

    }
}
