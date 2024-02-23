using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using OpenAI.Managers;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.AI
{
    public class OpenAIFilesPlugin(OpenAIService openAIService, ProactiveMessageService proactiveMessageService,
        AssistantService assistantService, IStorage storage, ConversationFilesService conversationFilesService,
        DriveRepository driveRepository) : OpenAIBasePlugin(openAIService, proactiveMessageService, driveRepository, "Files")
    {
        [Action("OpenAI.AddFileToConversation")]
        [Description("Adds a file to the current conversation")]
        [Parameter(name: "url", type: "string", required: true, description: "Url of the file to add")]
        public async Task<string> AddFileToConversation([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionParameters] Dictionary<string, object> parameters)
        {
            var url = parameters["url"]?.ToString();
            var attachment = new Models.File()
            {
                Filename = url!.GetFilenameFromUrl(),
                Url = url!
            };

            var currentFiles = turnState.Files ?? [];
            var newFile = await conversationFilesService.AddFileAsync(turnContext, attachment);

            if (newFile != null && newFile.Id != null && newFile.Filename != null)
            {
                var assistant = await assistantService.GetAssistantAsync(turnState.AssistantId);

                currentFiles.Add(newFile.Id);
                turnState.Files = currentFiles;
                turnState.EnsureTool(newFile.Filename.GetToolTypeFromFile()!, assistant.Tools);

                await turnState.SaveStateAsync(turnContext, storage);

                return "File added to the conversation";
            }

            return "File could not be added to the conversation";
        }
    }
}
