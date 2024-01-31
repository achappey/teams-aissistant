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
    public class GraphSitePermissionsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Site Permissions")
    {
        [Action("MicrosoftGraph.ListSitePermissions")]
        [Description("Lists site permissions for a SharePoint site")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        public Task<string> ListSitePermissions([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Sites[parameters["siteId"]?.ToString()].Permissions
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteSitePermissions")]
        [Description("Deletes SharePoint site permissions")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "permissionId", type: "string", required: true, visible: false, description: "Id of the permission")]
        public Task<string> DeleteSitePermissions([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var siteId = parameters["siteId"]?.ToString();
                    var permissionId = parameters["permissionId"]?.ToString();

                    var site = await graphClient.Sites[siteId].GetAsync();
                    var permission = await graphClient.Sites[siteId].Permissions[permissionId].GetAsync();

                    var siteName = site?.DisplayName ?? string.Empty;
                    var permissionNames = permission?.Roles != null ? string.Join(", ", permission.Roles) : string.Empty;
                    var identityCount = permission?.GrantedToIdentitiesV2?.Count.ToString() ?? "0";

                    var permissionInfo = $"{permissionNames}\n\nIdentities: {identityCount}";

                    return [
                        (new ParameterAttribute(name: "Site", type: "string", readOnly: true), siteName),
                        (new ParameterAttribute(name: "Permission", type: "string", readOnly: true), permissionInfo)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteSitePermissionsSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteSitePermissions", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Sites[jObject?["siteId"]?.ToString()].Permissions[jObject?["permissionId"]?.ToString()].DeleteAsync();

                    return "Site permissions deleted";
                }, cancellationToken);
        }


    }
}
