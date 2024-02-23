using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using OpenAI.Managers;

namespace TeamsAIssistant.Handlers.Plugins.AI
{
    public abstract class OpenAIBasePlugin(OpenAIService openAIService,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name) 
        : PluginBase(driveRepository, proactiveMessageService, name, "OpenAI", "API", "v1")
    {
        protected readonly OpenAIService _openAIService = openAIService;

        public async Task<string> ExecuteOpenAIQuery<T>(
                TurnContext turnContext, TeamsAIssistantState turnState, string actionName,
               Dictionary<string, object> parameters,
               Func<OpenAIService, Dictionary<string, object>, Task<T>> query)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            var result = await query(_openAIService, parameters);
            var json = JsonConvert.SerializeObject(result);

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, json, cardId);

            return json;
        }
    }
}
