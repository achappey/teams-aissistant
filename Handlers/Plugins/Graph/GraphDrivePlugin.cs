using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphDrivePlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "OneDrive")
    {

        [Action("MicrosoftGraph.GetMyOneDrive")]
        [Description("Gets current user OneDrive")]
        public Task<string> GetMyOneDrive([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Me.Drive
                            .GetAsync(conf =>
                            {
                                conf.QueryParameters.Expand = ["root"];
                            }));
        }

        [Action("MicrosoftGraph.GetOneDriveByGroupId")]
        [Description("Gets a OneDrive by a group id")]
        [Parameter(name: "groupId", type: "string", required: true, description: "Id of the group")]
        public Task<string> GetOneDriveByGroupId([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Groups[parameters["groupId"].ToString()].Drive
                            .GetAsync(conf =>
                            {
                                conf.QueryParameters.Expand = ["root"];
                            }));
        }

        /*  [Action("MicrosoftGraph.GetOneDriveBySiteId")]
          [Description("Gets a OneDrive by a site id")]
          [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
          public Task<string> GetOneDriveBySiteId([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
          {
              return ExecuteGraphQuery(
                  turnContext, turnState, actionName, parameters,
                  (graphClient, paramDict) => graphClient.Sites[parameters["siteId"].ToString()].Drive
                              .GetAsync(conf =>
                              {
                                  conf.QueryParameters.Expand = ["root"];
                              }));
          }
  */
        [Action("MicrosoftGraph.GetOneDrivesBySiteId")]
        [Description("Gets OneDrives by site id")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        public Task<string> GetOneDrivesBySiteId([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Sites[parameters["siteId"].ToString()].Drives
                            .GetAsync();

                        return result?.Value;
                    });
        }


        [Action("MicrosoftGraph.GetOneDriveChildren")]
        [Description("Gets children of a drive item")]
        [Parameter(name: "driveId", type: "string", required: true, description: "Id of the drive")]
        [Parameter(name: "itemId", type: "string", required: true, description: "Id of the drive item")]
        public Task<string> GetMyOneDriveChildren([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Drives[parameters["driveId"].ToString()]
                            .Items[parameters["itemId"].ToString()].Children
                            .GetAsync();

                        return result?.Value;
                    });
        }


    }
}
