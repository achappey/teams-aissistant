using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphWorkplaceManagementPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Workplace Management")
    {
        [Action("MicrosoftGraph.ListProfileCardProperties")]
        [Description("Get a collection of profileCardProperty resources for an organization")]
        public Task<string> ListProfileCardProperties([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Admin.People.ProfileCardProperties.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListPronounsSettings")]
        [Description("Get the properties of the pronounsSettings resource for an organization")]
        public Task<string> ListPronounsSettings([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Admin.People.Pronouns.GetAsync());
        }
    }
}