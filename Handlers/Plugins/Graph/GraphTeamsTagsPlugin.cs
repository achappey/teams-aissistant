using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsTagsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Tags")
    {

        [Action("MicrosoftGraph.ListTeamsTags")]
        [Description("List tags from a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamsTags([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Tags
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateTeamsTag")]
        [Description("Creates a new teams tag in a Teams")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Display name of the tag")]
        [Parameter(name: "members", type: "string", required: true, description: "Comma separated list of user ids")]
        public Task<string> CreateTeamsTag([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamsId = parameters["teamsId"]?.ToString();
                    var team = await graphClient.Teams[teamsId].GetAsync();

                    var members = parameters["members"]?.ToString()?.Split(",");
                    var users = await graphClient.Users.GetByIds.PostAsGetByIdsPostResponseAsync(new()
                    {
                        Ids = members?.ToList()
                    });

                    var teamName = team?.DisplayName ?? string.Empty;
                    var memberNames = string.Join(",", users?.Value?.Select(user => (user as User)?.DisplayName) ?? []);

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "Members", type: "string", readOnly: true), memberNames)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateTeamsTagSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTeamsTag", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new TeamworkTag
                    {
                        DisplayName = jObject?["displayName"]?.ToString(),
                        Members = jObject?["members"]?.ToString()?.Split(",").Select(h => new TeamworkTagMember
                        {
                            UserId = h,
                        }).ToList(),
                    };

                    var result = await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                        .Tags.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdateTeamsTag")]
        [Description("Updates a tag in a Teams")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "tagId", type: "string", required: true, visible: false, description: "Id of the tag")]
        [Parameter(name: "displayName", type: "string", required: true, description: "New display name of the tag")]
        public Task<string> UpdateTeamsTag([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamsId = parameters["teamsId"]?.ToString();
                    var team = await graphClient.Teams[teamsId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphUpdateTeamsTagSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateTeamsTag", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new TeamworkTag
                    {
                        DisplayName = jObject?["displayName"]?.ToString(),
                    };

                    var result = await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                        .Tags[jObject?["tagId"]?.ToString()].PatchAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteTeamsTag")]
        [Description("Deletes a tag in a Teams")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "tagId", type: "string", required: true, visible: false, description: "Id of the tag")]
        public async Task<string> DeleteTeamsTag([ActionTurnContext] TurnContext turnContext,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return await SendGraphConfirmationCard(turnContext, actionName, parameters,
            async (GraphServiceClient graphClient) =>
            {
                var teamsId = parameters["teamsId"]?.ToString();
                var team = await graphClient.Teams[teamsId].GetAsync();
                var tag = await graphClient.Teams[teamsId].Tags[parameters["tagId"]?.ToString()].GetAsync();

                var teamName = team?.DisplayName ?? string.Empty;
                var tagName = tag?.DisplayName ?? string.Empty;

                return [
                    (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                    (new ParameterAttribute(name: "Tag", type: "string", readOnly: true), tagName)
                ];
            });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTeamsTagSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTeamsTag", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                        .Tags[jObject?["tagId"]?.ToString()].DeleteAsync();

                    return "Tag deleted";
                }, cancellationToken);
        }
    }
}
