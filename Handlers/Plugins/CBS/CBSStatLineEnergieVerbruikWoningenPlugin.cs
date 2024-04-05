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
    public class CBSStatLineEnergieVerbruikWoningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Energie Verbruik Woningen")
    {
        private const string Identifier = "81528NED";

        [Action("CBS.Energie.VerbruikWoningen.GetVerbruikWoningenByWoningkenmerken")]
        [Description("Deze tabel geeft regionale gegevens over het gemiddelde energieverbruik per woning (aardgas en elektriciteit) van particuliere woningen onderverdeeld naar verschillende woningtypen en type eigendom voor Nederland, de landsdelen, provincies en gemeentes. Daarnaast is alleen voor totaal woningen het percentage stadsverwarming opgenomen, omdat dit relevant is voor de interpretatie van de hoogte van het gemiddeld aardgasverbruik.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "woningkenmerken", type: "string", required: true, description: "Key of the woningkenmerken")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetVerbruikWoningenByWoningkenmerken([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "WoningkenmerkenCodes", parameters["woningkenmerken"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and Woningkenmerken eq '{parameters["woningkenmerken"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Energie.VerbruikWoningen.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Energie.VerbruikWoningen.GetWoningkenmerkenCodes")]
        [Description("Deze tabel bevat woningkenmerkenCodes")]
        public Task<string> GetWoningkenmerkenCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "WoningkenmerkenCodes", parameters);
        }
    }
}
