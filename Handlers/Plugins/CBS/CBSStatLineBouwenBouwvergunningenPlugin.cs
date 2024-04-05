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
    public class CBSStatLineBouwenBouwvergunningenPlugin(ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, TeamsAdapter teamsAdapter)
        : CBSStatLineBasePlugin(proactiveMessageService, driveRepository, teamsAdapter, "Bouwen Bouwvergunningen")
    {
        private const string Identifier = "83672NED";

        [Action("CBS.Bouwen.Bouwvergunningen.GetBouwvergunningenByGebouwsoort")]
        [Description("In deze tabel worden gegevens gepubliceerd over het aantal verleende bouwvergunningen en de geschatte bouwkosten van verleende bouwvergunningen voor bedrijfsgebouwen. De uitkomsten hebben betrekking op verleende bouwvergunningen met een bouwsom vanaf 50 duizend euro, exclusief BTW. De gegevens zijn uitgesplitst naar aard werkzaamheden (nieuwbouw en overig), naar gebouwsoort, naar bestemming (SBI 2008) en naar regio.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "bestemmingbedrijfsgebouwenSBI2008", type: "string", required: true, description: "Key of the bestemmingbedrijfsgebouwenSBI2008")]
        [Parameter(name: "gebouwsoort", type: "string", required: true, description: "Key of the gebouwsoort")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetBouwvergunningenByGebouwsoort([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var oppervlakteklasseValidation = await ValidateStatLineBaseParameter(Identifier, "BestemmingBedrijfsgebouwenSBI2008Codes", parameters["bestemmingbedrijfsgebouwenSBI2008"]?.ToString()!);

            if (oppervlakteklasseValidation != null)
            {
                return oppervlakteklasseValidation;
            }

            var eigenaarhuuderValidation = await ValidateStatLineBaseParameter(Identifier, "GebouwsoortCodes", parameters["gebouwsoort"]?.ToString()!);

            if (eigenaarhuuderValidation != null)
            {
                return eigenaarhuuderValidation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, Identifier, parameters["year"]?.ToString()!, parameters, $" and Gebouwsoort eq '{parameters["gebouwsoort"]?.ToString()!}' and BestemmingBedrijfsgebouwenSBI2008 eq '{parameters["bestemmingbedrijfsgebouwenSBI2008"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Bouwen.Bouwvergunningen.GetBouwvergunningenByEigendomCode")]
        [Description("In deze tabel worden gegevens gepubliceerd over het voortschrijdend jaargemiddelde van de bouwkosten, inhoud en oppervlakte van nieuw te bouwen woningen waarvoor een bouwvergunning is verleend. De uitkomsten hebben betrekking op verleende bouwvergunningen voor woningen met een totale bouwsom vanaf 50 duizend euro, exclusief BTW over de afgelopen twaalf maanden. Verleende bouwvergunningen waarin een mix van woningen met wooneenheden, recreatiewoningen en/of bedrijfsruimten in voorkomen worden in deze statistiek niet meegenomen.")]
        [Parameter(name: "year", type: "string", required: true, description: "Year of the cijfers")]
        [Parameter(name: "eigendom", type: "string", required: true, description: "Key of the eigendom")]
        [Parameter(name: "regioS", type: "string", required: true, description: "Key of the regio")]
        public async Task<string> GetBouwvergunningenByEigendomCode([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var validation = await ValidateStatLineBaseParameter("83673NED", "EigendomCodes", parameters["eigendom"]?.ToString()!);

            if (validation != null)
            {
                return validation;
            }

            return await ExecuteStatLineQuery(turnContext, turnState, actionName, "83673NED", parameters["year"]?.ToString()!, parameters, $" and Eigendom eq '{parameters["eigendom"]?.ToString()!}' and startswith(RegioS,'{parameters["regioS"]?.ToString()!}')");
        }

        [Action("CBS.Bouwen.Bouwvergunningen.GetRegios")]
        [Description("Deze tabel bevat regios")]
        public Task<string> GetRegios([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "RegioSCodes", parameters);
        }

        [Action("CBS.Bouwen.Bouwvergunningen.GetBestemmingBedrijfsgebouwenSBI2008Codes")]
        [Description("Deze tabel bevat bestemmingbedrijfsgebouwenSBI2008")]
        public Task<string> GetBestemmingBedrijfsgebouwenSBI2008Codes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "BestemmingBedrijfsgebouwenSBI2008Codes", parameters);
        }

        [Action("CBS.Bouwen.Bouwvergunningen.GetGebouwsoortCodes")]
        [Description("Deze tabel bevat gebouwsoort")]
        public Task<string> GetGebouwsoortCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, Identifier, "GebouwsoortCodes", parameters);
        }

        [Action("CBS.Bouwen.Bouwvergunningen.GetEigendomCodes")]
        [Description("Deze tabel bevat eigendomcodes")]
        public Task<string> GetEigendomCodes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteStatLineBaseQuery(turnContext, turnState, actionName, "83673NED", "EigendomCodes", parameters);
        }
    }
}
