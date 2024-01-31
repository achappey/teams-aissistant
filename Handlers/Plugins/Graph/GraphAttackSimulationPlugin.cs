using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSecurityAttackSimulationPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Attack Simulation")
    {

        [Action("MicrosoftGraph.ListAttackSimulationCampaigns")]
        [Description("Lists attack simulation campaigns with Microsoft Graph")]
        public Task<string> ListAttackSimulationCampaigns([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.AttackSimulation.Simulations
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListAttackSimulationAutomations")]
        [Description("Lists attack simulation automations with Microsoft Graph")]
        public Task<string> ListAttackSimulationAutomations([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.AttackSimulation.SimulationAutomations
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListAttackSimulationEndUserNotifications")]
        [Description("Lists attack simulation end user noifications with Microsoft Graph")]
        public Task<string> ListAttackSimulationEndUserNotifications([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.AttackSimulation.EndUserNotifications
                            .GetAsync();

                        return result?.Value;
                    });
        }


    }
}
