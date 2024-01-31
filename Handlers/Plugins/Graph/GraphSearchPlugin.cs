using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta.Search.Query;
using Microsoft.Graph.Beta.Models;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSearchPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Search")
    {
        [Action("MicrosoftGraph.SearchDocumentContent")]
        [Description("Search for document and site content with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        [Parameter(name: "query", type: "string", required: true, description: "Search query")]
        public Task<string> SearchDocumentContent([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchContent(turnContext, turnState, actionName, parameters, [EntityType.Drive, EntityType.DriveItem, EntityType.Site]);

        }

        [Action("MicrosoftGraph.SearchMessageContent")]
        [Description("Search for message content with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        [Parameter(name: "query", type: "string", required: true, description: "Search query")]
        public Task<string> SearchMessageContent([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchContent(turnContext, turnState, actionName, parameters, [EntityType.Message]);
        }

        [Action("MicrosoftGraph.SearchChatMessageContent")]
        [Description("Search for message content with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        [Parameter(name: "query", type: "string", required: true, description: "Search query")]
        public Task<string> SearchChatMessageContent([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchContent(turnContext, turnState, actionName, parameters, [EntityType.ChatMessage]);
        }

        private Task<string> SearchContent([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters, List<EntityType?>? entityTypes)
        {
            int from = 0;
            int size = 10;

            if (parameters.TryGetValue("skip", out var fromValue) && fromValue is int v1)
            {
                from = v1;
            }

            if (parameters.TryGetValue("top", out var sizeValue) && sizeValue is int v)
            {
                size = v;
            }

            var requestBody = new QueryPostRequestBody
            {
                Requests =
                [
                    new SearchRequest
                    {
                        EntityTypes = entityTypes,
                        Query = new SearchQuery
                        {
                            QueryString = parameters["query"].ToString(),
                        },
                        From = from,
                        Size = size,
                    },
                ],
            };

            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Search.Query
                            .PostAsQueryPostResponseAsync(requestBody);

                        return result?.Value?
                                        .SelectMany(y => y.HitsContainers ?? [])
                                        .SelectMany(y => y.Hits ?? [])
                                        .ToList();
                    });
        }


    }
}
