using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphApplicationsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Applications")
    {
        [Action("MicrosoftGraph.GetApplications")]
        [Description("Get the list of applications in this organization with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Display name of the application")]
        public Task<string> GetApplications([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Applications
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphSearchString();
                                });

                        return result?.Value;
                    });
        }
    }
}
