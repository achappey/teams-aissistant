using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphUserPermissionGrantsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "User Permission Grants")
    {
        [Action("MicrosoftGraph.ListUserPermissionGrants")]
        [Description("List all resource-specific permission grants on a user")]
        [Parameter(name: "userId", type: "string", description: "Id of the user. Defaults to current user")]
        public Task<string> ListUserPermissionGrants([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                        ? await graphClient.Users[value.ToString()].PermissionGrants.GetAsync()
                        : await graphClient.Me.PermissionGrants.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetMyOauth2PermissionGrants")]
        [Description("Retrieve a list of oAuth2PermissionGrant entities, which represent delegated permissions granted to enable a client application to access an API on behalf of the user")]
        [Parameter(name: "userId", type: "string", description: "Id of the user. Defaults to current user")]
        public Task<string> GetMyOauth2PermissionGrants([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
             turnContext, turnState, actionName, parameters,
             async (graphClient, paramDict) =>
                 {
                     var result = parameters.TryGetValue("userId", out object? value)
                         ? await graphClient.Users[value.ToString()].Oauth2PermissionGrants.GetAsync()
                         : await graphClient.Me.Oauth2PermissionGrants.GetAsync();

                     return result?.Value;
                 });
        }

    }
}
