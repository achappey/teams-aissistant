using Microsoft.Bot.Schema;
using TeamsAIssistant.Extensions;
using AdaptiveCards;
using TeamsAIssistant.Config;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Services;

public class ProactiveMessageService(TeamsAdapter adapter, IConfiguration configuration)
{
    private readonly string? _appId = configuration.Get<ConfigOptions>()!.BOT_ID;

    public async Task<string?> SendOrUpdateCardAsync(
        ConversationReference conversationReference,
        Func<AdaptiveCard> createCardFunc,
        string? updateCardId,
        CancellationToken cancellationToken)
    {
        string? activityId = updateCardId;

        await adapter.ContinueConversationAsync(_appId, conversationReference, async (turnContext, currentCancellationToken) =>
        {
            var card = createCardFunc().ToAdaptiveCardAttachment();

            if (activityId == null)
            {
                var response = await turnContext.SendActivityAsync(card, cancellationToken);
                activityId = response.Id;
            }
            else
            {
                card.Id = activityId;
                await turnContext.UpdateActivityAsync(card, cancellationToken);
            }

        }, cancellationToken);

        return activityId;


    }
}