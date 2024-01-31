using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailSettingsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Settings")
    {

        [Action("MicrosoftGraph.GetMyMailboxSettings")]
        [Description("Gets my mailbox settings from Outlook with Microsoft Graph")]
        public Task<string> GetMyMailboxSettings([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Me.MailboxSettings.GetAsync());
        }

        [Action("MicrosoftGraph.GetMyMailboxSupportedLanguages")]
        [Description("Gets my mailbox supported languages")]
        public Task<string> GetMyMailboxSupportedLanguages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
              async (graphClient, paramDict) =>
                {
                    var result = await graphClient.Me.Outlook.SupportedLanguages.GetAsSupportedLanguagesGetResponseAsync();

                    return result?.Value;
                });
        }

        [Action("MicrosoftGraph.GetMyMailboxSupportedTimeZones")]
        [Description("Gets my mailbox supported time zones")]
        public Task<string> GetMyMailboxSupportedTimeZones([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                {
                    var result = await graphClient.Me.Outlook.SupportedTimeZones.GetAsSupportedTimeZonesGetResponseAsync();

                    return result?.Value;
                });
        }
    }
}
