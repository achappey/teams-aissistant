using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using Microsoft.Teams.AI.AI;
using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using TeamsAIssistant.AdaptiveCards;
using Microsoft.KernelMemory;

namespace TeamsAIssistant.Handlers
{
    public class ActionHandlers(ConversationFilesService conversationFilesService, ProactiveMessageService proactiveMessageService)
    {

        [Action(AIConstants.UnknownActionName)]
        public Task<string> UnknownAction([ActionTurnState] TeamsAIssistantState state,
         [ActionName] string action)
        {
            return Task.Run(() =>
            {
                state.Temp.Input += $"Unknown function: {action}. Please check the function name carefully";
                return AIConstants.SayCommand;
            });
        }

        [Action(AIConstants.HttpErrorActionName)]
        public async Task<string> OnHttpError([ActionTurnContext] ITurnContext turnContext)
        {
            await turnContext.SendActivityAsync("An AI request failed. Please try again later.");
            return AIConstants.StopCommand;
        }

        [Action(Constants.AIConstants.FilePathActionName)]
        public async Task<string> DownloadFile(
            [ActionTurnContext] ITurnContext turnContext,
            [ActionTurnState] TeamsAIssistantState turnState,
            [ActionParameters] Dictionary<string, object> parameters)
        {
            // Extract the filename from the parameters dictionary.
            var filename = parameters["filename"].ToString();

            // Check if the user is authenticated, the filename is not null or empty,
            // and the file content is available as a byte array.
            if (turnState.IsAuthenticated() && !string.IsNullOrEmpty(filename)
                && parameters["fileContent"] is byte[] fileContent)
            {
                await conversationFilesService.SaveFile(turnContext, new()
                {
                    Filename = filename,
                    Content = fileContent
                });
            }

            return string.Empty;
        }

        [Action(Constants.AIConstants.CitationActionName)]
        public async Task<string> ShowCitation(
            [ActionTurnContext] ITurnContext turnContext,
            [ActionParameters] Dictionary<string, object> parameters)
        {
            var dads = parameters["citation"].ToString();
            var citationsCard = new CitationCardData(new(turnContext.Activity.Locale))
            {
                Citation = parameters["citation"] as Citation
            };

            await proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                            () => CitationCard.CitationCardTemplate.RenderAdaptiveCard(citationsCard),
                             null, CancellationToken.None);

            return string.Empty;
        }
    }
}
