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
    public class CBSStatLineMilieuDierlijkMestPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Milieu Dierlijk Mest")
    {
        private const string Identifier = "83983NED";

        [Action("CBS.Milieu.DierlijkMest.GetDierlijkMestByBedrijfsType")]
        [Description("Deze tabel bevat cijfers over de mestproductie en de daarmee uitgescheiden hoeveelheid stikstof en fosfaat. Daarnaast wordt de mestproductie vergeleken met de plaatsingsruimte volgens de geldende gebruiksnormen.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "bedrijfstype", type: "string", required: true, description: "Key of the bedrijfstype")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetDierlijkMestByBedrijfsType([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter(Identifier, "BedrijfstypeCodes", parameters["bedrijfstype"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and Bedrijfstype eq '{parameters["bedrijfstype"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Milieu.DierlijkMest.GetDierlijkMestByDiercategorie")]
        [Description("Deze tabel bevat cijfers over de mestproductie en de daarmee uitgescheiden hoeveelheid stikstof en fosfaat. De mestproductie wordt naar soort mest onderscheiden. De soorten mest en de mineralenuitscheiding worden uitgesplitst naar verschillende soorten vee binnen de veestapel.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "diercategorie", type: "string", required: true, description: "Key of the diercategorie")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetDierlijkMestByDiercategorie([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter("83982NED", "DiercategorieCodes", parameters["diercategorie"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, "83982NED", parameters["year"]?.ToString()!, parameters, $" and Diercategorie eq '{parameters["diercategorie"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Milieu.DierlijkMest.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Milieu.DierlijkMest.GetBedrijfstypeCodes")]
        [Description("Deze tabel bevat bedrijfstypeCodes")]
        public Task<string> GetBedrijfstypeCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "BedrijfstypeCodes", parameters);
        }

        [Action("CBS.Milieu.DierlijkMest.GetDiercategorieCodes")]
        [Description("Deze tabel bevat diercategorieCodes")]
        public Task<string> GetDiercategorieCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83982NED", "DiercategorieCodes", parameters);
        }
    }
}
