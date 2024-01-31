using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsAppsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Apps")
    {

        [Action("MicrosoftGraph.ListTeamPermissionGrants")]
        [Description("List all resource-specific permission grants on a team")]
        [Parameter(name: "teamId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamPermissionGrants([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamId"].ToString()].PermissionGrants.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListInstalledTeamsApps")]
        [Description("List all installed apps in a team")]
        [Parameter(name: "teamId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamApps([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamId"].ToString()].InstalledApps.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListInstalledUserTeamsApps")]
        [Description("List all installed teams apps of a user")]
        [Parameter(name: "userId", type: "string", required: true, description: "Id of the user")]
        public Task<string> ListInstalledUserTeamsApps([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Users[parameters["userId"].ToString()].Teamwork.InstalledApps.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetUserTeamsAppChat")]
        [Description("Gets a chat between a user and an app")]
        [Parameter(name: "userId", type: "string", required: true, description: "Id of the user")]
        [Parameter(name: "appId", type: "string", required: true, description: "Id of the app")]
        public Task<string> GetUserTeamsAppChat([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                      [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Users[parameters["userId"].ToString()]
                            .Teamwork.InstalledApps[parameters["appId"].ToString()].Chat.GetAsync();

                        return result;
                    });
        }

        [Action("MicrosoftGraph.RemoveTeamsAppFromTeam")]
        [Description("Removes an app from an a team")]
        [Parameter(name: "teamId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "appId", type: "string", required: true, visible: false, description: "Id of the app")]
        public Task<string> RemoveTeamsAppFromTeam([ActionTurnContext] TurnContext turnContext,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamId = parameters["teamId"]?.ToString();
                    var appId = parameters["appId"]?.ToString();

                    var team = await graphClient.Teams[teamId].GetAsync();
                    var app = await graphClient.Teams[teamId].InstalledApps[appId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;
                    var appName = app?.TeamsApp?.DisplayName ?? string.Empty;

                    return new List<(ParameterAttribute, string)>
                    {
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "App", type: "string", readOnly: true), appName)
                    };
                });
        }

        [Submit]
        public Task MicrosoftGraphRemoveTeamsAppFromTeamSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.RemoveTeamsAppFromTeam", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Teams[jObject?["teamId"]?.ToString()]
                        .InstalledApps[jObject?["appId"]?.ToString()]
                        .DeleteAsync();

                    return "App removed";
                }, cancellationToken);
        }


    }
}
