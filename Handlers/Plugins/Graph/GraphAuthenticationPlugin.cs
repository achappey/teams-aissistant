using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphAuthenticationPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Authentication")
    {

        [Action("MicrosoftGraph.ListUserRegistrationDetails")]
        [Description("Get a list of the authentication methods")]
        public Task<string> ListUserRegistrationDetails([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Reports.AuthenticationMethods.UserRegistrationDetails
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListUserRegisteredByFeature")]
        [Description("Get the number of users capable of multi-factor authentication, self-service password reset, and passwordless authentication")]
        public Task<string> ListUserRegisteredByFeature([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Reports.AuthenticationMethods.UsersRegisteredByFeature
                            .GetAsync();

                        return result;
                    });
        }

        [Action("MicrosoftGraph.ListUserRegisteredByMethod")]
        [Description("Get the number of users registered for each authentication method")]
        public Task<string> ListUserRegisteredByMethod([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Reports.AuthenticationMethods.UsersRegisteredByMethod
                            .GetAsync();

                        return result;
                    });
        }

    }
}
