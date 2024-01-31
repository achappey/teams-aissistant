using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphLicensesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Licenses")
    {

        [Action("MicrosoftGraph.ListSubscribedSkus")]
        [Description("Get the list of commercial subscriptions that an organization has acquired")]
        public Task<string> ListSubscribedSkus([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.SubscribedSkus
                            .GetAsync();

                        return result?.Value;
                    });
        }

    }
}
