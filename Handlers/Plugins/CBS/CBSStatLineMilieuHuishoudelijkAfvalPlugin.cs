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
    public class CBSStatLineMilieuHuishoudelijkAfvalPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Milieu Huishoudelijk Afval")
    {
        private const string Identifier = "83452NED";

        [Action("CBS.Milieu.HuishoudelijkAfval.GetHuishoudelijkAfvalByAfvalsoort")]
        [Description("Deze tabel toont de hoeveelheden ingezameld huishoudelijk afval per gemeente. De hoeveelheid huishoudelijk afval per inwoner kan per gemeente sterk variëren. Hiervoor zijn meerdere oorzaken aan te wijzen. Zo zal in een gemeente met veel hoogbouw minder GFT-afval en grof tuinafval vrijkomen omdat er minder tuinen zijn. In gemeenten met een diftar-systeem, waarbij de huishoudens meer moeten betalen als ze meer afval afgeven, komt vaak minder afval per inwoner vrij. Toeristische gemeenten zamelen vaak meer afval in. Dit laatste is vooral zichtbaar op de Waddeneilanden waar relatief veel afval per inwoner vrijkomt.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "afvalsoort", type: "string", required: true, description: "Key of the afvalsoort")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetHuishoudelijkAfvalByAfvalsoort([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "AfvalsoortCodes", parameters["afvalsoort"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and Afvalsoort eq '{parameters["afvalsoort"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Milieu.HuishoudelijkAfval.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Milieu.HuishoudelijkAfval.GetAfvalsoortCodes")]
        [Description("Deze tabel bevat afvalsoortCodes")]
        public Task<string> GetAfvalsoortCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "AfvalsoortCodes", parameters);
        }
    }
}
