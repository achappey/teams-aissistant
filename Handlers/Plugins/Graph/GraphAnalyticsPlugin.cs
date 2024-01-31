using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphAnalyticsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Analytics")
    {
        [Action("MicrosoftGraph.GetAnalyticsActivities")]
        [Description("Gets time spent by a user on various work activities during and outside of working hours")]
        public Task<string> GetAnalyticsActivities([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Analytics.ActivityStatistics
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToFilterString();
                                });

                        return result?.Value;
                    });
        }
    }
}
