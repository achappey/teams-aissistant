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
    public class CBSStatLineVoorraadGebouwenWoningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Voorraad Gebouwen Woningen")
    {
        [Action("CBS.VoorraadGebouwen.Woningen.GetVoorraadWoningenByOppervlakte")]
        [Description("Deze tabel bevat gegevens over de kenmerken van de voorraad woningen op 1 januari. De tabel toont de beginstand op 1 januari. Deze beginstand wordt verder uitgesplitst naar oppervlakteklasse, woningtype, landsdelen, provincies, COROP-gebieden en gemeenten")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "oppervlakteklasse", type: "string", required: true, description: "Key of the oppervlakteklasse")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        [Parameter(name: "woningtype", type: "string", required: true, description: "Key of the woningtype")]
        public async Task<string> GetVoorraadWoningenByOppervlakte([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var woningtypeValidation = await ValidateStatLineBaseParameter("83704NED", "WoningtypeCodes", parameters["woningtype"]?.ToString()!);

            if (woningtypeValidation != null)
            {
                return woningtypeValidation;
            }

            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter("83704NED", "OppervlakteklasseCodes", parameters["oppervlakteklasse"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, "83704NED", parameters["year"]?.ToString()!, parameters, $" and Woningtype eq '{parameters["woningtype"]?.ToString()!}' and Oppervlakteklasse eq '{parameters["oppervlakteklasse"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.VoorraadGebouwen.Woningen.GetVoorraadWoningenByBouwjaar")]
        [Description("Deze tabel bevat gegevens over de kenmerken van de voorraad woningen op 1 januari. De tabel toont de beginstand op 1 januari en de gemiddelde oppervlakte. Deze worden verder uitgesplitst naar bouwjaarklasse, woningtype, landsdelen, provincies, COROP-gebieden en gemeenten")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "bouwjaarklasse", type: "string", required: true, description: "Key of the bouwjaarklasse")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        [Parameter(name: "woningtype", type: "string", required: true, description: "Key of the woningtype")]
        public async Task<string> GetVoorraadWoningenByBouwjaar([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var woningtypeValidation = await ValidateStatLineBaseParameter("82550NED", "WoningtypeCodes", parameters["woningtype"]?.ToString()!);

            if (woningtypeValidation != null)
            {
                return woningtypeValidation;
            }

            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter("82550NED", "BouwjaarklasseCodes", parameters["bouwjaarklasse"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, "82550NED", parameters["year"]?.ToString()!, parameters, $" and Woningtype eq '{parameters["woningtype"]?.ToString()!}' and Bouwjaarklasse eq '{parameters["bouwjaarklasse"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.VoorraadGebouwen.Woningen.GetOppervlakteklasses")]
        [Description("Deze tabel bevat oppervlakteklasses")]
        public Task<string> GetOppervlakteklasses([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83704NED", "OppervlakteklasseCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.Woningen.GetBouwjaarklasses")]
        [Description("Deze tabel bevat bouwjaarklasses")]
        public Task<string> GetBouwjaarklasses([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "82550NED", "BouwjaarklasseCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.Woningen.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83704NED", "RegioSCodes", parameters);
        }

        [Action("CBS.VoorraadGebouwen.Woningen.GetWoningtypeCodes")]
        [Description("Deze tabel bevat woningtypecodes")]
        public Task<string> GetWoningtypeCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83704NED", "WoningtypeCodes", parameters);
        }
    }
}
