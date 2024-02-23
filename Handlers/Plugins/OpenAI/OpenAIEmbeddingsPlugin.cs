using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using OpenAI.Managers;

namespace TeamsAIssistant.Handlers.Plugins.AI
{
    public class OpenAIEmbeddingsPlugin(OpenAIService openAIService, ProactiveMessageService proactiveMessageService,
        GraphClientServiceProvider graphClientServiceProvider, IndexService indexService,
        DriveRepository driveRepository) : OpenAIBasePlugin(openAIService, proactiveMessageService, driveRepository, "Embeddings")
    {
        [Action("ReindexSiteVectors")]
        [Description("Runs a full index on a site vector database")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        public async Task<string> ReindexSiteVectors([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            await SendFunctionCard(turnContext, actionName, parameters);

            if (graphClientServiceProvider == null || !graphClientServiceProvider.IsAuthenticated())
            {
                return "Not authenticated";
            }

            await indexService.AddSiteToVectorIndex(parameters["siteId"]?.ToString()!);

            return $"Index requested. Items will be indexed, but it can take a while.";
        }

        [Action("ReindexTeamVectors")]
        [Description("Runs a full index on a team vector database")]
        [Parameter(name: "teamId", type: "string", required: true, description: "Id of the team")]
        public async Task<string> ReindexTeamVectors([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            await SendFunctionCard(turnContext, actionName, parameters);

            if (graphClientServiceProvider == null || !graphClientServiceProvider.IsAuthenticated())
            {
                return "Not authenticated";
            }

            await indexService.AddTeamToVectorIndex(parameters["teamId"]?.ToString()!);

            return $"Index requested. Items will be indexed, but it can take a while.";
        }

        [Action("ReindexSimplicateVectors")]
        [Description("Runs a full index on the Simplicate vector database")]
        public async Task<string> ReindexSimplicateVectors([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                      [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            await SendFunctionCard(turnContext, actionName, parameters);

            await indexService.AddSimplicateVectorIndex();

            return $"Index requested. Items will be indexed, but it can take a while.";
        }

    }
}
