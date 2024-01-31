using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSitesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Sites")
    {
        [Action("MicrosoftGraph.SearchSites")]
        [Description("Search for SharePoint sites with Microsoft Graph")]
        [Parameter(name: "query", type: "string", required: true, description: "Search query")]
        public Task<string> SearchSites([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Sites
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters["query"].ToString();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetSiteAnalytics")]
        [Description("Gets the site analytics by site id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        public Task<string> GetSiteAnalytics([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Sites[parameters["siteId"].ToString()].Analytics.GetAsync());
        }

      

    }
}
