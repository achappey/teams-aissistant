using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphUsersPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Users")
    {
        private static readonly string[] QueryFields = [ "id", "displayName", "mail", "accountEnabled", "mobilePhone", "userType",
            "city", "companyName", "createdDateTime", "employeeId", "jobTitle", "department", "preferredLanguage" ];

        [Action("MicrosoftGraph.SearchUsers")]
        [Description("Search for member users with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Name of the user")]
        [Parameter(name: "department", type: "string", description: "Department of the user")]
        [Parameter(name: "userType", type: "string", description: "User type such as Member or Guest")]
        public Task<string> SearchUsers([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Users
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Select = QueryFields;
                                    requestConfiguration.QueryParameters.Filter = parameters.ToGraphUserFilterString();
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphUserSearchString();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetMyProfile")]
        [Description("Gets the current user profile with Microsoft Graph")]
        public Task<string> GetMyProfile([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Me.Profile.GetAsync());
        }

        [Action("MicrosoftGraph.GetMyUserSettings")]
        [Description("Gets the current user settings")]
        public Task<string> GetMyUserSettings([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Me.Settings.GetAsync());
        }

       

    }
}
