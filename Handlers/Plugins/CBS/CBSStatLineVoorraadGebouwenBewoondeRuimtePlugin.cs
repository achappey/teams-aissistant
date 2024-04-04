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
    public class CBSStatLineVoorraadGebouwenBewoondeRuimtePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Voorraad Gebouwen Bewoonde Ruimte")
    {
        private const string Identifier = "85058NED";

        [Action("CBS.VoorraadGebouwen.BewoondeRuimte.GetVoorraadWoningenBySoortBewoondeRuimte")]
        [Description("Deze tabel bevat gegevens over de bewoonde woonruimten op 1 januari, samengesteld uit verschillende bronnen. Deze tabel geeft inzicht in het aantal woningen, niet-woningen en stand- en ligplaatsen in Nederland die geregistreerd staan in de Basisregistraties Adressen en Gebouwen (BAG), als ook objecten die buiten deze registratie vallen en waar op het betreffende adres personen staan ingeschreven. Bij standplaatsen kan worden gedacht aan stacaravans. Bij ligplaatsen gaat het meestal om woonboten")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "soortbewoondewoonruimten", type: "string", required: true, description: "Key of the soortbewoondewoonruimten")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetVoorraadWoningenBySoortBewoondeRuimte([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter(Identifier, "SoortBewoondeWoonruimtenCodes", parameters["soortbewoondewoonruimten"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and SoortBewoondeWoonruimten eq '{parameters["soortbewoondewoonruimten"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.VoorraadGebouwen.BewoondeRuimte.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.BewoondeRuimte.GetSoortBewoondeWoonruimtenCodes")]
        [Description("Deze tabel bevat soortbewoondewoonruimten")]
        public Task<string> GetStatusVanBewoningCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "SoortBewoondeWoonruimtenCodes", parameters);
        }
    }
}
