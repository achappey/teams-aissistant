using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.CBS
{
    public class CBSStatLineHernieuwbareEnergiePlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, IHttpClientFactory httpClientFactory)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, httpClientFactory, "Hernieuwbare Energie")
    {
        [Action("CBS.HernieuwbareEnergie.GetWindenergieOpLand")]
        [Description("In deze tabel zijn cijfers per provincie opgenomen over de capaciteit van windmolens en de gerealiseerde elektriciteitsproductie. De gegevens zijn exclusief de in zee opgestelde windmolens")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetWindenergieOpLand([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "70960ned", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.HernieuwbareEnergie.GetWarmtepompen")]
        [Description("Deze tabel bevat cijfers over aantallen warmtepompen, de capaciteit van deze warmtepompen en de hoeveelheid geproduceerde en verbruikte energie. De temperatuur van de warmte uit de bodem of buitenlucht is vaak niet hoog genoeg voor direct gebruik")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetWarmtepompen([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "85523NED", parameters["year"]?.ToString()!, parameters);
        }

        [Action("CBS.HernieuwbareEnergie.GetAardwarmteEnBodemenergie")]
        [Description("In deze tabel wordt weergegeven hoeveel warmte en koude er uit de bodem wordt onttrokken. Warmte is een vorm van energie. Het gebruik van koude uit de bodem vermijdt het verbruik van elektriciteit voor koeling. Er wordt onderscheid gemaakt in aardwarmte en bodemenergie, met of zonder warmtepompen")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        public Task<string> GetAardwarmteEnBodemenergie([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineQuery(turnContext, turnState, actionName, "82379NED", parameters["year"]?.ToString()!, parameters);
        }

    }
}
