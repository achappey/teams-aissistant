using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamworkPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Rooms")
    {
        private const string TeamworkDeviceId = "teamworkDeviceId";

        [Action("MicrosoftGraph.ListTeamworkDevices")]
        [Description("Lists teamwork devices with Microsoft Graph")]
        public Task<string> ListTeamworkDevices([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.Devices
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListTeamworkDeviceOperations")]
        [Description("Lists teamwork device operations with Microsoft Graph")]
        public Task<string> ListTeamworkDeviceOperations([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.Devices[parameters[TeamworkDeviceId]?.ToString()].Operations
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.GetTeamworkDeviceHealth")]
        [Description("Get teamwork device health with Microsoft Graph")]
        [Parameter(name: TeamworkDeviceId, type: "string", required: true, readOnly: true, description: "Id of the teamwork device")]
        public Task<string> GetTeamworkDeviceHealth([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.Devices[parameters[TeamworkDeviceId]?.ToString()]
                            .Health.GetAsync();

                        return result;
                    });
        }

        [Action("MicrosoftGraph.GetTeamworkDeviceConfiguration")]
        [Description("Get teamwork device configuration with Microsoft Graph")]
        [Parameter(name: TeamworkDeviceId, type: "string", required: true, readOnly: true, description: "Id of the teamwork device")]
        public Task<string> GetTeamworkDeviceConfiguration([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.Devices[parameters[TeamworkDeviceId]?.ToString()]
                            .Configuration.GetAsync();

                        return result;
                    });
        }

        [Action("MicrosoftGraph.GetTeamworkDeviceActivity")]
        [Description("Get teamwork device activity with Microsoft Graph")]
        [Parameter(name: TeamworkDeviceId, type: "string", required: true, readOnly: true, description: "Id of the teamwork device")]
        public Task<string> GetTeamworkDeviceActivity([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.Devices[parameters[TeamworkDeviceId]?.ToString()]
                            .Activity.GetAsync();

                        return result;
                    });
        }

        [Action("MicrosoftGraph.RestartTeamworkDevice")]
        [Description("Restarts a teamwork device with Microsoft Graph")]
        [Parameter(name: TeamworkDeviceId, type: "string", required: true, readOnly: true, visible: false, description: "Id of the teamwork device")]
        public Task<string> RestartTeamworkDevice([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamworkDeviceId = parameters[TeamworkDeviceId]?.ToString();
                    var device = await graphClient.Teamwork.Devices[teamworkDeviceId].GetAsync();

                    var displayName = device?.CurrentUser?.DisplayName ?? string.Empty;
                    var deviceType = device?.DeviceType ?? TeamworkDeviceType.Unknown;
                    var model = device?.HardwareDetail?.Model ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "DisplayName", type: "string", readOnly: true), displayName),
                        (new ParameterAttribute(name: "DeviceType", type: "string", readOnly: true), deviceType.ToString()),
                        (new ParameterAttribute(name: "Model", type: "string", readOnly: true), model)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphRestartTeamworkDeviceSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.RestartTeamworkDevice", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var deviceId = jObject?[TeamworkDeviceId]?.ToString();
                await graphClient.Teamwork.Devices[deviceId].Restart.PostAsync();

                return "Teamwork device restarted";
            }, cancellationToken);
        }
    }
}
