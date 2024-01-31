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
    public class GraphDevicesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Devices")
    {

        [Action("MicrosoftGraph.SearchDevices")]
        [Description("Search for devices with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Name of the device")]
        public Task<string> SearchDevices([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Devices
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphSearchString();
                                });

                        return result?.Value;
                    });
        }
    }
}
