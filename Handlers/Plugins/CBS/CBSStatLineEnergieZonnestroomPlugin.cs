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
    public class CBSStatLineEnergieZonnestroomPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Energie Zonnestroom")
    {
        private const string Identifier = "85005NED";

        [Action("CBS.Energie.Zonnestroom.GetZonnestroomBySectorEnVermogensklasse")]
        [Description("Deze tabel bevat cijfers over het aantal installaties, het opgesteld vermogen aan zonnepanelen en de productie van zonnestroom door deze installaties. De cijfers zijn uit te splitsen naar sector (bedrijven en woningen). Daarnaast is het mogelijk om de installaties en opgewekte elektriciteit uit te splitsen naar installatiegrootte. Zon op land is uitgesplitst naar klein (<= 15 kW) en groot (>15 kW) vermogen. Het groot vermogen is verder uit te splitsen naar zonnestroom op dak en op land (veld). Installaties op binnenwateren worden gerekend tot veldinstallaties, installaties boven een parkeerplaats tot de dakinstallaties. Het aantal installaties en het opgesteld vermogen kunnen worden uitgesplitst naar gemeente, provincie, landsdeel, RES-regio en subRES-regio (RES staat voor Regionale EnergieStrategie). De productie van zonnestroom kan niet naar gemeenteniveau worden uitgesplitst. Bij productie is subRES-regio dus het laatste regionale schaalniveau.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "sectorenvermogensklasse", type: "string", required: true, description: "Key of the sectorenvermogensklasse")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetBouwvergunningenByGebouwsoort([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "SectorEnVermogensklasseCodes", parameters["sectorenvermogensklasse"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and SectorEnVermogensklasse eq '{parameters["sectorenvermogensklasse"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Energie.Zonnestroom.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Energie.Zonnestroom.GetSectorEnVermogensklasseCodes")]
        [Description("Deze tabel bevat sectorenvermogensklasseCodes")]
        public Task<string> GetSectorEnVermogensklasseCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "SectorEnVermogensklasseCodes", parameters);
        }
    }
}
