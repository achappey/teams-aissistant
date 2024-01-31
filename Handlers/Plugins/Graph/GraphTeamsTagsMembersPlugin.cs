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
    public class GraphTeamsTagsMembersPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Tag Members")
    {
        [Action("MicrosoftGraph.ListTeamsTagMembers")]
        [Description("List tags members from a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        [Parameter(name: "tagId", type: "string", required: true, description: "Id of the tag")]
        public Task<string> ListTeamsTagMembers([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()]
                            .Tags[parameters["tagId"]?.ToString()].Members
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateTeamsTagMember")]
        [Description("Creates a new teams tag member")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "tagId", type: "string", required: true, visible: false, description: "Id of the tag")]
        [Parameter(name: "memberId", type: "string", required: true, visible: false, description: "User id of the member")]
        public Task<string> CreateTeamsTagMember([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var teamsId = parameters["teamsId"]?.ToString();
                    var memberId = parameters["memberId"]?.ToString();
                    var tagId = parameters["tagId"]?.ToString();

                    var team = await graphClient.Teams[teamsId].GetAsync();
                    var user = await graphClient.Users[memberId].GetAsync();
                    var tag = await graphClient.Teams[teamsId].Tags[tagId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;
                    var userName = user?.DisplayName ?? string.Empty;
                    var tagName = tag?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "Member", type: "string", readOnly: true), userName),
                        (new ParameterAttribute(name: "Tag", type: "string", readOnly: true), tagName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreateTeamsTagMemberSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateTeamsTagMember", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new TeamworkTagMember
                    {
                        UserId = jObject?["memberId"]?.ToString(),
                    };

                    var result = await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                        .Tags[jObject?["tagId"]?.ToString()].Members.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteTeamsTagMember")]
        [Description("Deletes a member from a tag")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "tagId", type: "string", required: true, visible: false, description: "Id of the tag")]
        [Parameter(name: "tagMemberId", type: "string", required: true, visible: false, description: "Id of the tag member")]
        public Task<string> DeleteTeamsTagMember([ActionTurnContext] TurnContext turnContext,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
            async (GraphServiceClient graphClient) =>
            {
                var teamsId = parameters["teamsId"]?.ToString();
                var memberId = parameters["memberId"]?.ToString();
                var tagId = parameters["tagId"]?.ToString();

                var team = await graphClient.Teams[teamsId].GetAsync();
                var user = await graphClient.Users[memberId].GetAsync();
                var tag = await graphClient.Teams[teamsId].Tags[tagId].GetAsync();

                var teamName = team?.DisplayName ?? string.Empty;
                var userName = user?.DisplayName ?? string.Empty;
                var tagName = tag?.DisplayName ?? string.Empty;

                return [
                    (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                    (new ParameterAttribute(name: "Member", type: "string", readOnly: true), userName),
                    (new ParameterAttribute(name: "Tag", type: "string", readOnly: true), tagName)
                ];
            });
        }

        [Submit]
        public Task MicrosoftGraphDeleteTeamsTagMemberSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteTeamsTagMember", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Teams[jObject?["teamsId"]?.ToString()]
                      .Tags[jObject?["tagId"]?.ToString()].Members[jObject?["tagMemberId"]?.ToString()].DeleteAsync();

                    return "Tag member deleted";
                }, cancellationToken);
        }
    }
}
