using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using PexelsDotNetSDK.Api;
using TeamsAIssistant.Config;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers.Plugins.Pexels
{
    public abstract class PexelsBasePlugin(IConfiguration configuration, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository, string name)
        : PluginBase(driveRepository, proactiveMessageService, name, "Pexels", "Image & Video", "v1")
    {
        protected readonly PexelsClient client = new(configuration.Get<ConfigOptions>()?.Pexels);

        protected async Task<string> SearchMediaAsync(ITurnContext turnContext, TeamsAIssistantState turnState,
            string actionName, Dictionary<string, object> parameters, Func<string, int, string?, string?, string?, Task<string>> searchFunction)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var query = parameters["query"].ToString();
            var page = parameters.TryGetValue("page", out object? value) ? int.Parse(value.ToString()!) : 1;
            var size = parameters.TryGetValue("size", out object? sizeValue) ? sizeValue.ToString() : null;
            var orientation = parameters.TryGetValue("orientation", out object? orientationValue) ? orientationValue.ToString() : null;
            var locale = parameters.TryGetValue("locale", out object? localeValue) ? localeValue.ToString() : null;

            try
            {
                string result = await searchFunction(query!, page, size, orientation, locale);
                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, result, cardId);

                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
