using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using OpenAI.Managers;
using Newtonsoft.Json;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class OpenAIFunctionsPlugin(OpenAIService openAIService, ProactiveMessageService proactiveMessageService,
        AssistantService assistantService, IStorage storage, PluginService pluginService,
        DriveRepository driveRepository) : OpenAIBasePlugin(openAIService, proactiveMessageService, driveRepository, "Functions")
    {
        [Action("OpenAI.SearchPlugins")]
        [Description("Gets a list of available plugins with names, functions and descriptions")]
        public async Task<string> SearchPlugins([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var plugins = pluginService.GetPlugins();
            var items = plugins?.Select(h => new
            {
                h.Name,
                h.ApiName,
                h.ApiVersion,
                FunctionCount = h?.Actions?.Count(),
            });

            var resultJson = JsonConvert.SerializeObject(items);
            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);

            return resultJson;
        }

        [Action("OpenAI.GetPluginFunctions")]
        [Description("Gets all functions of a plugin")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the plugin")]
        public async Task<string> GetPluginFunctions([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var plugins = pluginService.GetPlugins() ?? [];
            var plugin = plugins.FirstOrDefault(t => t.Name == parameters["name"].ToString());

            if (plugin == null)
            {
                return "Plugin does not exist";
            }
         
            var resultJson = JsonConvert.SerializeObject(plugin.Actions);
            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);

            return resultJson;
        }


        [Action("OpenAI.AddPlugin")]
        [Description("Adds a plugin to the conversation")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the plugin")]
        public async Task<string> AddPlugin([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var plugins = pluginService.GetPlugins() ?? [];
            var assistant = await assistantService.GetAssistantAsync(turnState.AssistantId);
            var plugin = plugins.FirstOrDefault(t => t.Name == parameters["name"].ToString());

            if (plugin == null)
            {
                return "Plugin does not exist";
            }

            turnState.AddPlugin(plugin, assistant.Tools);
            await turnState.SaveStateAsync(turnContext, storage);
            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, JsonConvert.SerializeObject(plugin.Actions), cardId);

            return $"Plugin {parameters["name"]} added";
        }

        [Action("OpenAI.RemovePlugin")]
        [Description("Removes a plugin from the conversation")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the plugin")]
        public async Task<string> RemovePlugin([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            await SendFunctionCard(turnContext, actionName, parameters);

            var plugins = pluginService.GetPlugins() ?? [];
            var plugin = plugins.FirstOrDefault(t => t.Name == parameters["name"].ToString());

            if (plugin == null || !turnState.Plugins.Contains(plugin.Name))
            {
                return "Plugin does not exist";
            }

            turnState.DeletePlugin(plugin);
            await turnState.SaveStateAsync(turnContext, storage);

            return $"Plugin {parameters["name"]} deleted";
        }

        [Action("OpenAI.GetConversationPlugins")]
        [Description("Gets a list of plugins currenly attached to the conversation")]
        public async Task<string> GetConversationPlugins([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            var assistant = await assistantService.GetAssistantAsync(turnState.AssistantId);
            var plugins = turnState.Plugins.Select(t => new { Name = t, Source = "Conversation", ReadOnly = false }).ToList();
            var assistantPlugins = assistant.GetPlugins()?.Split(",")?.Select(a => new { Name = a, Source = "Assistant", ReadOnly = true });
            plugins.AddRange(assistantPlugins ?? []);

            var resultJson = JsonConvert.SerializeObject(plugins);

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, resultJson, cardId);

            return resultJson;
        }
    }
}
