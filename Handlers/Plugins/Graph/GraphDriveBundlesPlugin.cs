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
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphDriveBundlesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "OneDrive Bundles")
    {

        [Action("MicrosoftGraph.GetOneDriveBundles")]
        [Description("Gets drive bundles")]
        [Parameter(name: "driveId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive")]
        [Parameter(name: "itemId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive item")]
        public Task<string> GetOneDriveBundles([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Drives[parameters["driveId"]?.ToString()].Bundles
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateOneDriveBundle")]
        [Description("Creates an OneDrive bundle")]
        [Parameter(name: "driveId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the bundle")]
        [Parameter(name: "album", type: "boolean", description: "Create a photo album")]
        [Parameter(name: "driveItemIds", type: "string", readOnly: true, visible: false, required: true, description: "Comma seperated list of drive item ids")]
        public Task<string> CreateOneDriveBundle([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var driveId = parameters["driveId"]?.ToString();
                    var drive = await graphClient.Drives[driveId].GetAsync();
                    var driveName = drive?.Name ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Drive", type: "string", readOnly: true), driveName),
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateOneDriveBundleSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateOneDriveBundle", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var driveId = jObject?["driveId"]?.ToString();
                    var driveItemIds = jObject?["driveItemIds"]?.ToString()?.Split(",");

                    var requestBody = new DriveItem
                    {
                        Name = jObject?["name"]?.ToString(),
                        Bundle = new Bundle
                        {
                            Album = jObject != null && jObject.ContainsKey("album") ? jObject?["album"]!.ToObject<bool>() == true ? new Album
                            {
                            } : null : null,
                        },
                        Children = driveItemIds?.Select(t => new DriveItem
                        {
                            Id = t,
                        }).ToList() ?? [],
                        AdditionalData = new Dictionary<string, object>
                        {
                            {
                                "@microsoft.graph.conflictBehavior" , "rename"
                            },
                        },
                    };

                    var result = await graphClient.Drives[driveId].Bundles.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }


    }
}
