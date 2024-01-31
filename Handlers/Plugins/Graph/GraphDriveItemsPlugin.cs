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
    public class GraphDriveItemsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "OneDrive Items")
    {

        [Action("MicrosoftGraph.GetDriveItemVersions")]
        [Description("Gets drive item versions")]
        [Parameter(name: "driveId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive")]
        [Parameter(name: "itemId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive item")]
        public Task<string> GetDriveItemVersions([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Drives[parameters["driveId"]?.ToString()].Items[parameters["itemId"]?.ToString()].Versions
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.SearchDriveItem")]
        [Description("Search for a drive item")]
        [Parameter(name: "driveId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive")]
        [Parameter(name: "query", type: "string", readOnly: true, visible: false, required: true, description: "Search query")]
        public Task<string> SearchDriveItem([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {

                        var result = await graphClient.Drives[parameters?["driveId"]?.ToString()].SearchWithQ(parameters?["query"]?.ToString()).GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteOneDriveItem")]
        [Description("Deletes an OneDrive item")]
        [Parameter(name: "driveId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive")]
        [Parameter(name: "itemId", type: "string", readOnly: true, visible: false, required: true, description: "Id of the drive item")]
        public Task<string> DeleteOneDriveItem([ActionTurnContext] TurnContext turnContext,
       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var driveId = parameters["driveId"]?.ToString();
                    var itemId = parameters["itemId"]?.ToString();

                    var drive = await graphClient.Drives[driveId].GetAsync();
                    var driveItem = await graphClient.Drives[driveId].Items[itemId].GetAsync();

                    var driveName = drive?.Name ?? string.Empty;
                    var itemName = driveItem?.Name ?? string.Empty;
                    var itemDescription = driveItem?.Description ?? string.Empty;
                    var itemSize = driveItem?.Size ?? double.NaN;
                    var itemType = driveItem?.File != null ? "File" : "Folder";

                    return [
                        (new ParameterAttribute(name: "Drive", type: "string", readOnly: true), driveName),
                        (new ParameterAttribute(name: "Item", type: "string", readOnly: true), itemName),
                        (new ParameterAttribute(name: "Description", type: "string", readOnly: true), itemDescription),
                        (new ParameterAttribute(name: "Size", type: "number", readOnly: true), itemSize.ToString()),
                        (new ParameterAttribute(name: "Type", type: "string", readOnly: true), itemType)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteOneDriveItemSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteOneDriveItem", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var driveId = jObject?["driveId"]?.ToString();
                    var itemId = jObject?["itemId"]?.ToString();

                    await graphClient.Drives[driveId].Items[itemId].DeleteAsync();

                    return "OneDrive item deleted";
                }, cancellationToken);
        }


    }
}
