using Microsoft.Bot.Builder;
using Microsoft.Teams.AI;
using TeamsAIssistant.AdaptiveCards;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    public class FileHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly FileService _fileService;
        private readonly ProactiveMessageService _proactiveMessageService;
        public readonly RouteHandler<TeamsAIssistantState> SourcesMessageHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> ShowFilesHandler;

        public FileHandlers(AssistantService assistantService, FileService fileService,
            ProactiveMessageService proactiveMessageService)
        {
            _assistantService = assistantService;
            _fileService = fileService;
            _proactiveMessageService = proactiveMessageService;

            SourcesMessageHandler = HandleSourcesMessageAsync;
            ShowFilesHandler = HandleShowFilesAsync;
        }

        private Task HandleShowFilesAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return ShowFilesCard(turnContext, turnState, cancellationToken, true);
        }

        private Task HandleSourcesMessageAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            return ShowFilesCard(turnContext, turnState, cancellationToken);
        }

        public async Task ShowFilesCard(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken, bool newCard = false)
        {
            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);

            var assistantFilesTask = FetchFilesAsync(assistant.FileIds);
            var conversationFilesTask = FetchFilesAsync(turnState.Files);

            await Task.WhenAll(assistantFilesTask, conversationFilesTask);

            var assistantFiles = await assistantFilesTask;
            var conversationFiles = await conversationFilesTask;

            FilesCardData filesCardData = new(new(turnContext.Activity.Locale))
            {
                AssistantName = assistant.Name,
                AssistantFiles = assistantFiles,
                ConversationFiles = conversationFiles,
                IsAssistantOwner = assistant.IsOwner(turnContext.Activity.From),
                ShowConversationFiles = turnState.IsAuthenticated()
            };

            await _proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                                     () => FileCards.FilesCardTemplate.RenderAdaptiveCard(filesCardData),
                                     newCard ? null : turnContext.Activity.ReplyToId, cancellationToken);
        }

        private async Task<List<Models.File>> FetchFilesAsync(IEnumerable<string> fileIds)
        {
            var tasks = fileIds.Select(_fileService.GetFileAsync).ToList();
            var files = await Task.WhenAll(tasks);
            return [.. files];
        }
    }
}
