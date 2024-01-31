using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphExternalConnectionsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "External Connections")
    {
        [Action("MicrosoftGraph.ListExternalConnections")]
        [Description("Lists external connections with Microsoft Graph")]
        public Task<string> ListExternalConnections([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.External.Connections
                            .GetAsync();

                        return result?.Value;
                    });
        }


    }
}
