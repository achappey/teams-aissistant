using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Newtonsoft.Json;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphListItemsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "List Items")
    {
        [Action("MicrosoftGraph.GetSiteListItems")]
        [Description("Gets the site list items by site and list id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        public Task<string> GetSiteListItems([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                {
                    var result = await graphClient.Sites[parameters["siteId"].ToString()].Lists[parameters["listId"].ToString()].Items
                        .GetAsync((requestConfig) =>
                        {
                            requestConfig.QueryParameters.Expand = ["fields"];
                        });

                    return result?.Value;
                });
        }

        [Action("MicrosoftGraph.CreateSiteListItem")]
        [Description("Creates a new item in a list on a SharePoint site")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "listId", type: "string", required: true, visible: false, description: "Id of the list")]
        [Parameter(name: "fields", type: "string", required: true, description: "Comma separated list of field names")]
        [Parameter(name: "values", type: "string", required: true, description: "Comma separated list of field values")]
        public Task<string> CreateSiteListItem([ActionTurnContext] TurnContext turnContext,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var siteId = parameters["siteId"]?.ToString();
                    var listId = parameters["listId"]?.ToString();

                    var site = await graphClient.Sites[siteId].GetAsync();
                    var list = await graphClient.Sites[siteId].Lists[listId].GetAsync();

                    var siteName = site?.DisplayName ?? string.Empty;
                    var listName = list?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Site", type: "string", readOnly: true), siteName),
                        (new ParameterAttribute(name: "List", type: "string", readOnly: true), listName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateSiteListItemSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateSiteListItem", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var fieldNames = jObject?["fields"]?.ToString()?.Split(",");
                    var fieldValues = jObject?["values"]?.ToString()?.Split(",");
                    var items = fieldNames?.Zip(fieldValues!, (name, value) => new { name, value })
                            .ToDictionary(x => x.name, x => (object)x.value);

                    var requestBody = new ListItem
                    {
                        Fields = new FieldValueSet()
                        {
                            AdditionalData = items
                        }
                    };

                    var item = await graphClient.Sites[jObject?["siteId"]?.ToString()]
                        .Lists[jObject?["listId"]?.ToString()].Items.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(item);
                }, cancellationToken);
        }
    }
}
