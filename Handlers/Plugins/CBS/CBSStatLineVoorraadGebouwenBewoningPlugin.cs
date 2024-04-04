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
    public class CBSStatLineVoorraadGebouwenBewoningPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Voorraad Gebouwen Bewoning")
    {
        private const string Identifier = "82900NED";

        [Action("CBS.VoorraadGebouwen.Bewoning.GetVoorraadWoningenByStatusVanBewoning")]
        [Description("Deze tabel bevat gegevens over het eigendom van de voorraad woningen op 1 januari, samengesteld uit verschillende bronnen. Deze tabel geeft inzicht in het eigendom van de woningvoorraad naar koop- en huurwoningen. Huurwoningen worden verder onderverdeeld naar woningen in eigendom van woningcorporaties en woningen in eigendom van overige verhuurders. Daarnaast geeft deze tabel inzicht in de status van bewoning; onderverdeeld naar aantal bewoonde- en niet bewoonde woningen. De gegevens worden verder uitgesplitst naar landsdelen, provincies, COROP-gebieden en gemeenten")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "statusvanbewoning", type: "string", required: true, description: "Key of the statusvanbewoning")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetVoorraadWoningenByStatusVanBewoning([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter(Identifier, "StatusVanBewoningCodes", parameters["statusvanbewoning"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and StatusVanBewoning eq '{parameters["statusvanbewoning"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.VoorraadGebouwen.Bewoning.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.Bewoning.GetStatusVanBewoningCodes")]
        [Description("Deze tabel bevat statusvanbewoningcodes")]
        public Task<string> GetStatusVanBewoningCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "StatusVanBewoningCodes", parameters);
        }
    }
}
