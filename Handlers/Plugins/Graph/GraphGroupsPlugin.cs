using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphGroupsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Groups")
    {

        [Action("MicrosoftGraph.SearchGroups")]
        [Description("Search for groups with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Name of the group")]
        public Task<string> SearchGroups([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Groups
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphSearchString();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.AddGroupMember")]
        [Description("Adds a member to a group")]
        [Parameter(name: "groupId", type: "string", required: true, visible: false, description: "Id of the group")]
        [Parameter(name: "userId", type: "string", required: true, visible: false, description: "AAD user id of the member")]
        public Task<string> AddGroupMember([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var userId = parameters["userId"]?.ToString();
                    var groupId = parameters["groupId"]?.ToString();

                    var user = await graphClient.Users[userId].GetAsync();
                    var group = await graphClient.Groups[groupId].GetAsync();

                    var groupDisplayName = group?.DisplayName ?? string.Empty;
                    var memberDisplayName = user?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Group", type: "string", readOnly: true), groupDisplayName),
                        (new ParameterAttribute(name: "Member", type: "string", readOnly: true), memberDisplayName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphAddGroupMemberSubmit(ITurnContext turnContext, TeamsAIssistantState turnState,
            object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.AddGroupMember", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var groupId = jObject?["groupId"]?.ToString();
                    var requestBody = new ReferenceCreate
                    {
                        OdataId = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}",
                    };

                    await graphClient.Groups[groupId].Members.Ref.PostAsync(requestBody);

                    return "Member added";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteGroup")]
        [Description("Deletes a group")]
        [Parameter(name: "groupId", type: "string", required: true, visible: false, description: "Id of the group")]
        public Task<string> DeleteGroup([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var groupId = parameters["groupId"]?.ToString();

                    var group = await graphClient.Groups[groupId].GetAsync();

                    var groupDisplayName = group?.DisplayName ?? string.Empty;
                    var groupDescription = group?.Description ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Group", type: "string", readOnly: true), groupDisplayName),
                        (new ParameterAttribute(name: "Description", type: "string", readOnly: true), groupDescription)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteGroupSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteGroup", data, 
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var groupId = jObject?["groupId"]?.ToString();
                    await graphClient.Groups[groupId].DeleteAsync();

                    return "Group deleted";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.GetDeletedGroups")]
        [Description("Gets deleted groups with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Name of the group")]
        public Task<string> GetDeletedGroups([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Directory.DeletedItems.GraphGroup
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphUserSearchString();
                                });

                        return result?.Value;
                    });
        }
    }
}
