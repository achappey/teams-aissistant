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
    public class GraphListColumnsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "List Columns")
    {
        [Action("MicrosoftGraph.CreateSiteListTextColumn")]
        [Description("Creates a new text column in a list on a SharePoint site")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "listId", type: "string", required: true, visible: false, description: "Id of the list")]
        [Parameter(name: "name", type: "string", required: true, description: "The API-facing name of the column as it appears in the [fields][] on a [listItem][]")]
        [Parameter(name: "displayName", type: "string", required: true, description: "The user-facing name of the column")]
        [Parameter(name: "description", type: "string", description: "The user-facing description of the column")]
        [Parameter(name: "allowMultipleLines", type: "boolean", description: "Whether to allow multiple lines of text")]
        [Parameter(name: "maxLength", type: "number", maximum: 255, description: "The maximum number of characters for the value")]
        [Parameter(name: "indexed", type: "boolean", description: "Specifies whether the column values can used for sorting and searching")]
        [Parameter(name: "textType", type: "string", enumValues: ["plain", "richText"], description: "The type of text being stored")]
        [Parameter(name: "linesForEditing", type: "number", description: "The size of the text box")]
        public Task<string> CreateSiteListTextColumn([ActionTurnContext] TurnContext turnContext,
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
        public Task MicrosoftGraphCreateSiteListTextColumnSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateSiteListColumn", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new ColumnDefinition
                    {
                        Name = jObject?["name"]?.ToString(),
                        DisplayName = jObject?["displayName"]?.ToString(),
                        Description = jObject?["description"]?.ToString(),
                        Indexed = jObject?.ContainsKey("indexed") == true ? jObject?["indexed"]?.ToObject<bool>() : null,
                        Text = new TextColumn()
                        {
                            TextType = jObject?.ContainsKey("textType") == true
                                ? jObject?["textType"]?.ToString() : null,
                            LinesForEditing = jObject?.ContainsKey("linesForEditing") == true
                                ? jObject?["linesForEditing"]?.ToObject<int>() : null,
                            AllowMultipleLines = jObject?.ContainsKey("allowMultipleLines") == true
                                ? jObject?["allowMultipleLines"]?.ToObject<bool>() : null,
                            MaxLength = jObject?.ContainsKey("maxLength") == true
                                ? jObject?["maxLength"]?.ToObject<int>() : null
                        }
                    };

                    var item = await graphClient.Sites[jObject?["siteId"]?.ToString()]
                        .Lists[jObject?["listId"]?.ToString()].Columns.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(item);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.GetSiteListColumns")]
        [Description("Gets the site list columns by site and list id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        [Parameter(name: "listId", type: "string", required: true, description: "Id of the list")]
        public Task<string> GetSiteListColumns([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
            turnContext, turnState, actionName, parameters,
            async (graphClient, paramDict) =>
                {
                    var result = await graphClient.Sites[parameters["siteId"].ToString()].Lists[parameters["listId"].ToString()].Columns
                        .GetAsync();

                    return result?.Value;
                });
        }
    }
}
