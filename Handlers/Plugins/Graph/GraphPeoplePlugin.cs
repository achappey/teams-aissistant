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
    public class GraphPeoplePlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "People")
    {
        [Action("MicrosoftGraph.ListRelevantPeople")]
        [Description("Retrieve a list of person objects ordered by their relevance to the user, which is determined by the user's communication and collaboration patterns, and business relationships")]
        [Parameter(name: "displayName", type: "string", description: "Name of the person")]
        [Parameter(name: "topic", type: "string", description: "Find people based on topics extracted from e-mail conversations with that person")]
        [Parameter(name: "department", type: "string", description: "Department of the person")]
        public Task<string> ListRelevantPeople([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.People
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToGraphUserFilterString();
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphUserSearchString();
                                });

                        return result?.Value;
                    });
        }
    }
}
