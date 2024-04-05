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
    public class CBSStatLineVoorraadGebouwenZorgWoonruimtePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Voorraad Gebouwen Zorgwoonruimten")
    {
        private const string Identifier = "85150NED";

        [Action("CBS.VoorraadGebouwen.ZorgWoonruimte.GetVoorraadWoningenByZorgWoonruimte")]
        [Description("De tabel bevat gegevens over het aantal zorgwoonruimten op 1 januari. Zorgwoonruimten zijn verblijfsobjecten waar institutionele huishoudens woonachtig zijn die zorg nodig hebben. Daaronder vallen verzorgings- en verpleeghuizen, instellingen voor geestelijke gezondheidszorg, forensische centra en instellingen voor verstandelijk, lichamelijk en zintuiglijk gehandicapten")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "zorgwoonruimte", type: "string", required: true, description: "Key of the zorgwoonruimte")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetVoorraadWoningenByZorgWoonruimte([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter(Identifier, "TypeZorgwoonruimteCodes", parameters["zorgwoonruimte"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and TypeZorgwoonruimte eq '{parameters["zorgwoonruimte"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.VoorraadGebouwen.ZorgWoonruimte.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.ZorgWoonruimte.GetTypeZorgwoonruimteCodes")]
        [Description("Deze tabel bevat zorgwoonruimtecodes")]
        public Task<string> GetTypeZorgwoonruimteCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "TypeZorgwoonruimteCodes", parameters);
        }
    }
}
