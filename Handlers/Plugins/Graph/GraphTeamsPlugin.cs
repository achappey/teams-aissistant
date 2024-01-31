using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams")
    {

        [Action("MicrosoftGraph.ListJoinedTeams")]
        [Description("Lists the joined teams of the current user with Microsoft Graph")]
        public Task<string> ListJoinedTeams([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.JoinedTeams
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateNewTeam")]
        [Description("Creates a new Team")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Name of the team")]
        [Parameter(name: "description", type: "string", required: true, description: "Description of the team")]
        [Parameter(name: "visibility", type: "string", required: true, enumValues: ["Public", "Private"], description: "Visibility of the team")]
        public Task<string> CreateNewTeam([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateNewTeamSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateNewTeam", data, async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new Team
                {
                    DisplayName = jObject?["displayName"]?.ToString(),
                    Visibility = Enum.Parse<TeamVisibilityType>(jObject?["visibility"]?.ToString()
                        ?? Enum.GetName(TeamVisibilityType.Public)!),
                    Description = jObject?["description"]?.ToString(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "template@odata.bind" , "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"
                        },
                    },
                };

                var result = await graphClient.Teams.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }
    }
}
