using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Teams.AI;

namespace TeamsAIssistant
{
    public class AdapterWithErrorHandler : TeamsAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<TeamsAdapter> logger)
               : base(configuration, null, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");
                // Send a message to the user
                await turnContext.SendActivityAsync($"Error: {exception.Message}");
                // Send a trace activity
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}