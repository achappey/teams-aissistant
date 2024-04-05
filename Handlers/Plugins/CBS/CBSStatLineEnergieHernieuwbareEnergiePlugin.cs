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
    public class CBSStatLineEnergieHernieuwbareEnergiePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Energie Hernieuwbaar")
    {
        private const string Identifier = "85004NED";

        [Action("CBS.Energie.Hernieuwbaar.GetHernieuwbareEnergieByBronEnTechniek")]
        [Description("Deze tabel bevat cijfers over het opgestelde vermogen (MW) en de productie (opgewekte elektriciteit, mln kWh) van windenergie op land en zonnestroom, vanaf 2018. De gegevens zijn exclusief de in zee opgestelde windmolens")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "bronentechniek", type: "string", required: true, description: "Key of the bronentechniek")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetHernieuwbareEnergieByBronEnTechniek([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "BronEnTechniekCodes", parameters["bronentechniek"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and BronEnTechniek eq '{parameters["bronentechniek"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Energie.Hernieuwbaar.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Energie.Hernieuwbaar.GetBronEnTechniekCodes")]
        [Description("Deze tabel bevat sectorenvermogensklasseCodes")]
        public Task<string> GetBronEnTechniekCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "BronEnTechniekCodes", parameters);
        }

        [Action("CBS.Energie.Hernieuwbaar.GetWarmtepompen")]
        [Description("Deze tabel bevat cijfers over aantallen warmtepompen, de capaciteit van deze warmtepompen en de hoeveelheid geproduceerde en verbruikte energie. De temperatuur van de warmte uit de bodem of buitenlucht is vaak niet hoog genoeg voor direct gebruik")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetWarmtepompen([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "85523NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.Energie.Hernieuwbaar.GetAardwarmteEnBodemenergie")]
        [Description("In deze tabel wordt weergegeven hoeveel warmte en koude er uit de bodem wordt onttrokken. Warmte is een vorm van energie. Het gebruik van koude uit de bodem vermijdt het verbruik van elektriciteit voor koeling. Er wordt onderscheid gemaakt in aardwarmte en bodemenergie, met of zonder warmtepompen")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetAardwarmteEnBodemenergie([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "82379NED", parameters["year"]?.ToString()!, parameters);
        }

    }
}
