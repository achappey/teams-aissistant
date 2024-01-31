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
    public class GraphTeamsMembersPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Members")
    {

        [Action("MicrosoftGraph.GetTeamMembers")]
        [Description("Gets team members with Microsoft Graph")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> GetTeamMembers([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Members
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Orderby = ["displayName"];
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.AddTeamMember")]
        [Description("Adds a member to a Microsoft Team")]
        [Parameter(name: "teamsId", type: "string", required: true, visible: false, description: "Id of the team")]
        [Parameter(name: "userId", type: "string", required: true, visible: false, description: "AAD user id of the member")]
        public Task<string> AddTeamMember([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var userId = parameters["userId"]?.ToString();
                    var teamId = parameters["teamsId"]?.ToString();

                    var user = await graphClient.Users[userId].GetAsync();
                    var team = await graphClient.Teams[teamId].GetAsync();

                    var teamName = team?.DisplayName ?? string.Empty;
                    var memberName = user?.DisplayName ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Team", type: "string", readOnly: true), teamName),
                        (new ParameterAttribute(name: "Member", type: "string", readOnly: true), memberName)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphAddTeamMemberSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.AddTeamMember", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var userId = jObject?["userId"]?.ToString();
                    var teamsId = jObject?["teamsId"]?.ToString();

                    var requestBody = new AadUserConversationMember
                    {
                        OdataType = "#microsoft.graph.aadUserConversationMember",
                        Roles =
                        [
                            "member",
                        ],
                        AdditionalData = new Dictionary<string, object>
                        {
                            {
                                "user@odata.bind" , $"https://graph.microsoft.com/v1.0/users('{userId}')"
                            },
                        },
                    };

                    var member = await graphClient.Teams[teamsId].Members.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(member);
                }, cancellationToken);
        }
    }
}
