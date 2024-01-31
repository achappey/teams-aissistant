using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using TeamsAIssistant.Attributes;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailCategoriesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Categories")
    {

        [Action("MicrosoftGraph.ListMyMailCategories")]
        [Description("List my mail categories from Outlook with Microsoft Graph")]
        public Task<string> ListMyMailCategories([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Outlook.MasterCategories
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateMailCategory")]
        [Description("Creates a mail category")]
        [Parameter(name: "color", type: "string", required: true, enumValues: ["None", "Preset0", "Preset1", "Preset2", "Preset3", "Preset4", "Preset5", "Preset6", "Preset7", "Preset8", "Preset9", "Preset10", "Preset11", "Preset12", "Preset13", "Preset14", "Preset15", "Preset16", "Preset17", "Preset18", "Preset19", "Preset20", "Preset21", "Preset22", "Preset23", "Preset24", "Preset25"],
                description: "Color preset number")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Display name of the category")]
        public Task<string> CreateMailCategory([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateMailCategorySubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateMailCategory", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var color = jObject?["color"]?.ToString();

                var requestBody = new OutlookCategory
                {
                    DisplayName = jObject?["displayName"]?.ToString(),
                    Color = Enum.Parse<CategoryColor>(color ?? Enum.GetName(CategoryColor.None)!),
                };

                var result = await graphClient.Me.Outlook.MasterCategories.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdateMailCategory")]
        [Description("Updates a mail category")]
        [Parameter(name: "categoryId", type: "string", required: true, visible: false, description: "Id of the category")]
        [Parameter(name: "color", type: "string", required: true, description: "Color preset number",
            enumValues: ["None", "Preset0", "Preset1", "Preset2", "Preset3", "Preset4", "Preset5", "Preset6", 
                "Preset7", "Preset8", "Preset9", "Preset10", "Preset11", "Preset12", "Preset13", "Preset14", 
                "Preset15", "Preset16", "Preset17", "Preset18", "Preset19", "Preset20", "Preset21", "Preset22", 
                "Preset23", "Preset24", "Preset25"])]
        public Task<string> UpdateMailCategory([ActionTurnContext] TurnContext turnContext,
                  [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var categoryId = parameters["categoryId"]?.ToString();
                    var category = await graphClient.Me.Outlook.MasterCategories[categoryId].GetAsync();

                    var categoryDisplayName = category?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Category", type: "string", readOnly: true), categoryDisplayName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphUpdateMailCategorySubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateMailCategory", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var color = jObject?["color"]?.ToString();

                    var requestBody = new OutlookCategory
                    {
                        Color = Enum.Parse<CategoryColor>(color ?? Enum.GetName(CategoryColor.None) ?? "None"),
                    };

                    var result = await graphClient.Me.Outlook.MasterCategories[jObject?["categoryId"]?.ToString()].PatchAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteMailCategory")]
        [Description("Deleted a mail category")]
        [Parameter(name: "categoryId", type: "string", required: true, visible: false, description: "Id of the category")]
        public Task<string> DeleteMailCategory([ActionTurnContext] TurnContext turnContext,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var categoryId = parameters["categoryId"]?.ToString();
                    var category = await graphClient.Me.Outlook.MasterCategories[categoryId].GetAsync();

                    var categoryDisplayName = category?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Category", type: "string", readOnly: true), categoryDisplayName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteMailCategorySubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteMailCategory", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Outlook.MasterCategories[jObject?["categoryId"]?.ToString()].DeleteAsync();

                    return "Category deleted";
                }, cancellationToken);
        }
    }
}
