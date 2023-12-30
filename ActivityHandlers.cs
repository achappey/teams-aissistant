using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.Planners.Experimental;
using Newtonsoft.Json;

namespace MathBot
{
    /// <summary>
    /// Defines the activity handlers.
    /// </summary>
    public class ActivityHandlers
    {
        private readonly AssistantService _assistantService;

        private readonly string _assistantId;

        public RouteHandler<AssistantsState> SourcesMessageHandler;
        
        public RouteHandler<AssistantsState> AssistantMessageHandler;

        public ActivityHandlers(AssistantService assistantService, IConfiguration configuration)
        {
            _assistantService = assistantService;
            _assistantId = configuration.Get<ConfigOptions>()!.OpenAI!.AssistantId!;

            SourcesMessageHandler = HandleSourcesMessageAsync;
            AssistantMessageHandler = GetAssistantAsync;
        }

        public async Task HandleSourcesMessageAsync(ITurnContext turnContext, AssistantsState turnState, CancellationToken cancellationToken)
        {
            var assistant = await _assistantService.GetAssistantAsync(_assistantId);

            List<TeamsAIssistant.Models.File> files = new();

            foreach (var fileId in assistant.FileIds)
            {
                var file = await _assistantService.GetFileAsync(fileId);
                files.Add(file);
            }

            AdaptiveCard card = AdaptiveCardCreator.CreateFileListCard(files);

            Attachment attachment = new()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(card.ToJson())
            };

            var reply = MessageFactory.Attachment(attachment);

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        public async Task GetAssistantAsync(ITurnContext turnContext, AssistantsState turnState, CancellationToken cancellationToken)
        {
            var assistant = await _assistantService.GetAssistantAsync(_assistantId);

            AdaptiveCard card = AdaptiveCardCreator.CreateAssistantInfoCard(assistant);

            Attachment attachment = new()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(card.ToJson())
            };

            var reply = MessageFactory.Attachment(attachment);

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Handles "/reset" message.
        /// </summary>
        public static RouteHandler<AssistantsState> ResetMessageHandler = async (ITurnContext turnContext, AssistantsState turnState, CancellationToken cancellationToken) =>
        {
            turnState.DeleteConversationState();
            await turnContext.SendActivityAsync("Ok lets start this over.", cancellationToken: cancellationToken);
        };
    }
}
