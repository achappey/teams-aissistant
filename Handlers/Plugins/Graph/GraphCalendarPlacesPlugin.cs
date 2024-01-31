using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphCalendarPlacesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Calendar Places")
    {
        [Action("MicrosoftGraph.ListRooms")]
        [Description("Lists rooms")]
        public Task<string> ListRooms([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Places.GraphRoom.GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListRoomLists")]
        [Description("Lists room lists")]
        public Task<string> ListRoomLists([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
               [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Places.GraphRoomList.GetAsync();

                        return result?.Value;
                    });
        }


    }
}
