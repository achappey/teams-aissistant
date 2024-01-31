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
    public class GraphListsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Lists")
    {

        [Action("MicrosoftGraph.GetSiteLists")]
        [Description("Gets the site lists by site id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        public Task<string> GetSiteLists([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
            turnContext, turnState, actionName, parameters,
            async (graphClient, paramDict) =>
                {
                    var result = await graphClient.Sites[parameters["siteId"].ToString()].Lists
                        .GetAsync();

                    return result?.Value;
                });
        }

        [Action("MicrosoftGraph.CreateSiteList")]
        [Description("Creates a list on a SharePoint site")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "title", type: "string", required: true, description: "Title of the new list")]
        [Parameter(name: "template", type: "string", required: true, description: "Template of the list",
            enumValues: ["genericList", "documentLibrary", "events"])]
        public Task<string> CreateSiteList([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var siteId = parameters["siteId"]?.ToString();

                    var site = await graphClient.Sites[siteId].GetAsync();
                    var siteName = site?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Site", type: "string", readOnly: true), siteName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateSiteListSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateSiteList", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new List
                    {
                        DisplayName = jObject?["title"]?.ToString(),
                        ListProp = new ListInfo
                        {
                            Template = jObject?["template"]?.ToString(),
                        },
                    };

                    var list = await graphClient.Sites[jObject?["siteId"]?.ToString()].Lists.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(list);
                }, cancellationToken);
        }
    }
}
