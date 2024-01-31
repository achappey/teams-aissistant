using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineSocialeZekerheidPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Sociale Zekerheid")
    {
        [Action("CBS.SocialeZekerheid.GetKerncijfers")]
        [Description("Deze tabel geeft een actueel overzicht van de belangrijkste statistische cijfers over de sociale zekerheid. De cijfers hebben betrekking op uitkeringen in het kader van arbeidsongeschiktheid, werkloosheid, bijstand, ouderen, nabestaanden en kinderbijslag (de volksverzekeringen AOW, Anw en AKW)")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the kerncijfers")]
        public Task<string> GetKerncijfers([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "37789ksz", parameters["year"]?.ToString()!, parameters);
        }

    }
}
