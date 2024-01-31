using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using Microsoft.Teams.AI.AI;
using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    public class ActionHandlers(ConversationFilesService conversationFilesService)
    {

        [Action(AIConstants.UnknownActionName)]
        public async Task<string> UnknownAction([ActionTurnContext] ITurnContext turnContext, [ActionName] string action)
        {
            await turnContext.SendActivityAsync($"An AI request failed: {action}. Please try again.");
            return AIConstants.StopCommand;
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
                await conversationFilesService.SaveFile(turnContext, new Models.File()
                {
                    Filename = filename,
                    Content = fileContent
                });
            }

            return string.Empty;
        }

    }
}
