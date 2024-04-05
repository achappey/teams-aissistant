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
    public class CBSStatLinePrijzenHuurwoningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Prijzen Huurwoningen")
    {
        [Action("CBS.PrijzenHuurwoningen.GetHuurverhogingenWoningenNederland")]
        [Description("Deze tabel bevat cijfers over de gemiddelde huurverhoging (in- en exclusief huurharmonisatie) van gereguleerde- en geliberaliseerde huurwoningen. De gegevens zijn uitgesplitst naar landsdeel, provincie en de 4 grote gemeenten (Amsterdam, Rotterdam, Den Haag en Utrecht).")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetHuurverhogingenWoningenNederland([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "83162NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.PrijzenHuurwoningen.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83162NED", "RegioSCodes", parameters);
        }


    }
}
