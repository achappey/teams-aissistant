using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphNotesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Notebook")
    {

        [Action("MicrosoftGraph.ListNotebooks")]
        [Description("Lists notebooks with Microsoft Graph")]
        public Task<string> ListNotebooks([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Onenote.Notebooks
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListNotebookSections")]
        [Description("Lists notebook sections with Microsoft Graph")]
        public Task<string> ListNotebookSections([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Onenote.Sections
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListNotebookPages")]
        [Description("Lists notebook pages with Microsoft Graph")]
        public Task<string> ListNotebookPages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.Onenote.Pages
                            .GetAsync();

                        return result?.Value;
                    });
        }
    }
}
