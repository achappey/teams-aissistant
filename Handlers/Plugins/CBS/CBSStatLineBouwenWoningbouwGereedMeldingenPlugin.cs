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
    public class CBSStatLineBouwenWoningbouwGereedMeldingenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Bouwen Woningbouw")
    {
        private const string Identifier = "82213NED";

        [Action("CBS.Bouwen.Woningbouw.GetGereedMeldingenByProjectgrootte")]
        [Description("Deze tabel bevat gegevens over de doorlooptijd van de bouw van gereedgemelde woningen, afgeleid uit de Basisregistratie Adressen en Gebouwen (BAG). De gegevens worden verder uitgesplitst naar landsdelen, provincies, COROP-gebieden en gemeenten.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "projectgrootte", type: "string", required: true, description: "Key of the projectgrootte")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetBouwvergunningenByGebouwsoort([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "ProjectgrootteCodes", parameters["projectgrootte"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and Projectgrootte eq '{parameters["projectgrootte"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Bouwen.Woningbouw.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Bouwen.Woningbouw.GetProjectgrootteCodes")]
        [Description("Deze tabel bevat projectgrootteCodes")]
        public Task<string> GetProjectgrootteCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "ProjectgrootteCodes", parameters);
        }
    }
}
