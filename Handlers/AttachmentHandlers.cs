using Microsoft.Bot.Builder;
using Microsoft.Teams.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    public class AttachmentHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly FileService _fileService;
        private readonly IStorage _storage;
        private readonly ConversationFilesService _conversationFilesService;
        public readonly RouteHandler<TeamsAIssistantState> ExportMessagesHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> DeleteFileHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> DeleteAssistantFileHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> AttachFileHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> AddToChatFileHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> ExportButtonHandler;
        public readonly FileHandlers _fileHandlers;

        public AttachmentHandlers(AssistantService assistantService, IStorage storage,
            FileService fileService, ConversationFilesService conversationFilesService,
            FileHandlers fileHandlers)
        {
            _assistantService = assistantService;
            _fileService = fileService;
            _conversationFilesService = conversationFilesService;
            _fileHandlers = fileHandlers;
            _storage = storage;

            DeleteFileHandler = HandleDeleteFileAsync;
            DeleteAssistantFileHandler = HandleDeleteAssistantFileAsync;
            AttachFileHandler = HandleAttachFileAsync;
            AddToChatFileHandler = HandleAddToChatAsync;
            ExportMessagesHandler = HandleExportMessagesAsync;
            ExportButtonHandler = HandleExportMessageButtonAsync;
        }

        private async Task HandleAddToChatAsync(ITurnContext turnContext, TeamsAIssistantState turnState, 
            object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            var fileUrl = jObject["FileUrl"]?.Value<string>();
            var filename = jObject["Filename"]?.Value<string>();

            var file = new Models.File()
            {
                Url = fileUrl!,
                Filename = filename!
            };

            var currentFiles = turnState.Files ?? [];

            var newFile = await _conversationFilesService.AddFileAsync(turnContext, file);

            if (newFile != null)
            {
                var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);

                if (newFile.Id != null && newFile.Filename != null)
                {
                    currentFiles.Add(newFile.Id);
                    turnState.Files = currentFiles;
                    turnState.EnsureTool(newFile.Filename.GetToolTypeFromFile()!, assistant.Tools);

                    await turnState.SaveStateAsync(turnContext, _storage);
                }
            }
        }


        private async Task HandleDeleteFileAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var deleteFileId = JObject.FromObject(data)[AssistantForm.FileId]?.Value<string>();

            if (deleteFileId != null)
            {
                await _fileService.DeleteFileAsync(deleteFileId);
                turnState.Files = turnState.Files.Where(t => t != deleteFileId).ToList();

                await _fileHandlers.ShowFilesCard(turnContext, turnState, cancellationToken);
            }
        }

        private async Task HandleDeleteAssistantFileAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var deleteFileId = JObject.FromObject(data)[AssistantForm.FileId]?.Value<string>();

            if (deleteFileId != null)
            {
                await _assistantService.DeleteAssistantFileAsync(deleteFileId, turnState.AssistantId);
                await _fileService.DeleteFileAsync(deleteFileId);
                turnState.Files = turnState.Files.Where(t => t != deleteFileId).ToList();
                await _fileHandlers.ShowFilesCard(turnContext, turnState, cancellationToken);
            }
        }

        private async Task HandleAttachFileAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var attachFileIds = JObject.FromObject(data)[AssistantForm.FileIds]?.Value<string>()?.ToStringList();

            if (attachFileIds != null)
            {
                foreach (var attachFileId in attachFileIds)
                {
                    await _assistantService.CreateAssistantFileAsync(attachFileId, turnState.AssistantId);
                }

                turnState.Files = turnState.Files.Where(t => !attachFileIds.Contains(t)).ToList();
                await _fileHandlers.ShowFilesCard(turnContext, turnState, cancellationToken);
            }
        }

        private Task HandleExportMessageButtonAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return ExportMessagesAsync(turnContext, turnState, cancellationToken);
        }

        private async Task ExportMessagesAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            var messages = turnState.ThreadId != null ? await _assistantService.GetThreadMessagesAsync(turnState.ThreadId) : [];
            var messageJson = JsonConvert.SerializeObject(messages);
            var file = messageJson.ConvertJsonToCsv();

            if (file != null)
            {
                await _conversationFilesService.SaveFile(turnContext, new Models.File()
                {
                    Filename =  $"Messages-{DateTime.Now.Ticks}.csv",
                    Content = file
                });
            }
        }

        private Task HandleExportMessagesAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            return ExportMessagesAsync(turnContext, turnState, cancellationToken);
        }

        public async Task HandleAttachmentsAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            var currentFiles = turnState.Files ?? [];
            var validAttachments = turnContext.Activity.Attachments?.ToFiles() ?? [];
            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);

            foreach (Models.File newAttachment in validAttachments)
            {
                var newFile = await _conversationFilesService.AddFileAsync(turnContext, newAttachment);

                if (newFile != null && newFile.Id != null && newFile.Filename != null)
                {
                    currentFiles.Add(newFile.Id);
                    turnState.Files = currentFiles;
                    turnState.EnsureTool(newFile.Filename.GetToolTypeFromFile()!, assistant.Tools);
                }
            }
        }

    }
}
