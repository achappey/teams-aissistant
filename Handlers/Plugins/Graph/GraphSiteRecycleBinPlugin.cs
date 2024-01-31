using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSiteRecycleBinPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Site Recycle Bin")
    {
        [Action("MicrosoftGraph.ListSiteRecycleBin")]
        [Description("Lists recycle bin items on a SharePoint site")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> ListSiteRecycleBin([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                 {
                     var result = await graphClient.Sites[parameters["siteId"].ToString()].RecycleBin.Items
                         .GetAsync((requestConfiguration) =>
                             {
                                 requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                 requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                             });

                     return result?.Value;
                 });
        }

    }
}
