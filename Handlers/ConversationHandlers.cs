using System.Globalization;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.AdaptiveCards;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    /// <summary>
    /// Defines the activity handlers.
    /// </summary>
    public class ConversationHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly PluginService _pluginService;
        private readonly FileService _fileService;
        private readonly ProactiveMessageService _proactiveMessageService;
        public readonly ActionSubmitHandler<TeamsAIssistantState> UpdateConversationHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> UpdatePluginsHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> ResetConversationHandler;
        public readonly RouteHandler<TeamsAIssistantState> HandleResetMessageHandler;
        public readonly RouteHandler<TeamsAIssistantState> MenuHandler;
        public readonly RouteHandler<TeamsAIssistantState> WelcomeHandler;

        public ConversationHandlers(AssistantService assistantService, IConfiguration configuration,
            ProactiveMessageService proactiveMessageService, FileService fileService, PluginService pluginService)
        {
            _assistantService = assistantService;
            _fileService = fileService;
            _pluginService = pluginService;
            _proactiveMessageService = proactiveMessageService;

            HandleResetMessageHandler = HandleResetMessageAsync;
            UpdateConversationHandler = HandleUpdateConversationAsync;
            MenuHandler = HandleMenuAsync;
            WelcomeHandler = HandleWelcomeMessageAsync;
            ResetConversationHandler = HandleResetMessageAsync;
            UpdatePluginsHandler = HandleUpdatePluginsAsync;
        }

        private async Task HandleUpdatePluginsAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            UpdateTurnStateForPlugins(jObject, turnState);

            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);
            var newPlugins = jObject[AssistantForm.Plugins]?.Value<string>()?.Split(",");

            var updatedTools = UpdateToolsWithPlugins(newPlugins ?? [], assistant.Tools, turnState.Tools);
            turnState.Tools = updatedTools;
            turnState.Plugins = newPlugins?.ToList() ?? [];

            await ShowMenuAsync(turnContext, turnState, cancellationToken);
        }

        private void UpdateTurnStateForPlugins(JObject jObject, TeamsAIssistantState turnState)
        {
            turnState.CreateFunctionExports = jObject[AssistantForm.ExportFunctionOutput]?.Value<bool>() ?? false;
        }

        private Dictionary<string, Tool> UpdateToolsWithPlugins(string[] newPlugins, IEnumerable<Tool> assistantTools, Dictionary<string, Tool> currentTools)
        {
            var resultTools = new Dictionary<string, Tool>(currentTools);
            var pluginTools = GetToolsFromPlugins(newPlugins);

            // Remove non-function tools that are not in the new plugins list
            var nonFunctionTools = resultTools.Where(pair => pair.Value.Type == Tool.FUNCTION_CALLING_TYPE).ToList();
            foreach (var pair in nonFunctionTools)
            {
                if (newPlugins == null || !pluginTools.Any(a => a.Function?.Name == pair.Key))
                {
                    resultTools.Remove(pair.Key);
                }
            }

            foreach (var tool in pluginTools)
            {
                resultTools[tool.ToToolIdentifier()] = tool;
            }

            // Ensure function calling tools from the assistant are retained
            var assistantFunctionTools = assistantTools.Where(t => t.Type == Tool.FUNCTION_CALLING_TYPE);
            foreach (var tool in assistantFunctionTools)
            {
                var toolIdentifier = tool.ToToolIdentifier();
                if (!resultTools.ContainsKey(toolIdentifier))
                {
                    resultTools[toolIdentifier] = tool;
                }
            }

            return resultTools;
        }

        private Task<ResourceResponse> HandleWelcomeMessageAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(turnContext.Activity.From.Name))
            {
                return turnContext.SendActivityAsync($"Welcome at {turnContext.Activity.Recipient.Name}", cancellationToken: cancellationToken);
            }
            else
            {
                return turnContext.SendActivityAsync($"Hi {turnContext.Activity.From.Name}, welcome at {turnContext.Activity.Recipient.Name}", cancellationToken: cancellationToken);
            }
        }

        public Task HandleMenuAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            return ShowMenuAsync(turnContext, turnState, cancellationToken, true);
        }

        private async Task ShowMenuAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken, bool newCard = false)
        {
            var (assistant, assistants) = await FetchAssistants(turnContext, turnState);

            var tools = turnState.Tools.Count != 0 ? turnState.Tools.Where(a => !a.Value.IsFunctionTool()).Select(t => t.Key)
                : assistant.Tools.GetNonFunctionTools().Select(t => t.ToToolIdentifier());
            var visibleTools = tools.Where(r => r != Tool.FUNCTION_CALLING_TYPE);
            var messages = turnState.ThreadId != null ? await _assistantService.GetThreadMessagesAsync(turnState.ThreadId) : [];
            var allPlugins = _pluginService.GetPlugins() ?? [];
            IEnumerable<string> assistantPlugins = assistant.GetMetadataValue(AssistantMetadata.Plugins)?.Split(",") ?? [];
            double? usage = 0;

            if (turnState.ThreadId != null)
            {
                var totals = await _assistantService.GetThreadUsageAsync(turnState.ThreadId);
                
                foreach (var (model, input, output) in totals)
                {
                    usage += AIPricing.CalculateCost(model, input, output);
                }
            }

            MenuCardData menuCard = new(new CultureInfo(turnContext.Activity.Locale))
            {
                Assistant = assistant,
                IsAuthenticated = turnState.IsAuthenticated(),
                AssistantPlugins = assistantPlugins,
                Assistants = assistants?.Select(t => new AdaptiveChoice() { Value = t.Id, Title = t.Name! }),
                AdditionalInstructions = turnState.AdditionalInstructions ?? string.Empty,
                Tools = tools,
                Usage = (usage ?? 0).ToString("C", CultureInfo.CreateSpecificCulture("en-US")),
                ExportToolCalls = turnState.ExportToolCalls ?? false,
                AllPlugins = allPlugins.Select(t => t.Name),
                ConversationPlugins = turnState.Plugins,
                FileCount = assistant.FileIds.Count + turnState.Files.Count,
                PrependDateTime = turnState.PrependDateTime ?? false,
                PrependUsername = turnState.PrependUsername ?? false,
                Model = turnState.GetModel(assistant),
                MessageCount = messages.Count(),
                ExportFunctionOutput = turnState.CreateFunctionExports ?? false,
                BotName = turnContext.Activity.Recipient.Name,

            };

            var dsadas = JsonConvert.SerializeObject(menuCard);
            await _proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                               () => MenuCard.MenuCardTemplate.RenderAdaptiveCard(menuCard),
                                newCard ? null : turnContext.Activity.ReplyToId, cancellationToken);
        }

        private async Task<(Assistant assistant, IEnumerable<Assistant>? assistants)> FetchAssistants(ITurnContext turnContext, TeamsAIssistantState turnState)
        {
            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);
            var assistants = turnContext.Activity.From.AadObjectId != null
                ? await _assistantService.GetAssistantsAsync(turnContext.Activity.From.AadObjectId) : null;

            return (assistant, assistants);
        }

        private List<Tool> GetToolsFromPlugins(IEnumerable<string> pluginNames)
        {
            var toolsFromPlugins = new List<Tool>();

            foreach (var pluginName in pluginNames)
            {
                toolsFromPlugins.AddRange(_pluginService.GetPluginTools(pluginName));
            }

            return toolsFromPlugins;
        }

        private async Task HandleUpdateConversationAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            UpdateTurnStateFromJObject(jObject, turnState);

            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);
            var newTools = jObject[AssistantForm.Tools]?.Value<string>()?.Split(",");

            var updatedTools = UpdateTools(newTools ?? [], assistant.Tools, turnState.Tools);
            turnState.Tools = updatedTools;

            await ShowMenuAsync(turnContext, turnState, cancellationToken);
        }

        private static void UpdateTurnStateFromJObject(JObject jObject, TeamsAIssistantState turnState)
        {
            turnState.AssistantId = jObject[AssistantForm.AssistantId]?.Value<string>() ?? string.Empty;
            turnState.Model = jObject[AssistantForm.ModelId]?.Value<string>() ?? string.Empty;
            turnState.PrependDateTime = jObject[AssistantForm.PrependDateTime]?.Value<bool>() ?? false;
            turnState.PrependUsername = jObject[AssistantForm.PrependUsername]?.Value<bool>() ?? false;
            turnState.ExportToolCalls = jObject[AssistantForm.ExportToolCalls]?.Value<bool>() ?? false;
            turnState.AdditionalInstructions = jObject[AssistantForm.AdditionalInstructionsId]?.Value<string>() ?? string.Empty;
        }

        private static Dictionary<string, Tool> UpdateTools(string[] newTools, IEnumerable<Tool> assistantTools, Dictionary<string, Tool> currentTools)
        {
            var resultTools = new Dictionary<string, Tool>();

            // Add or update tools based on newTools array
            if (newTools != null)
            {
                foreach (var toolName in newTools)
                {
                    var tool = toolName.GetToolFromType();
                    resultTools[tool.ToToolIdentifier()] = tool;
                }
            }

            // Retain function calling tools from current state
            var functionTools = currentTools.Where(pair => pair.Value.Type == Tool.FUNCTION_CALLING_TYPE);
            foreach (var pair in functionTools)
            {
                resultTools[pair.Key] = pair.Value;
            }

            // Add missing function calling tools from assistant
            var assistantFunctionTools = assistantTools.Where(t => t.Type == Tool.FUNCTION_CALLING_TYPE);
            foreach (var tool in assistantFunctionTools)
            {
                var toolIdentifier = tool.ToToolIdentifier();
                if (!resultTools.ContainsKey(toolIdentifier))
                {
                    resultTools[toolIdentifier] = tool;
                }
            }

            return resultTools;
        }

        private Task HandleResetMessageAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return HandleResetMessage(turnContext, turnState, cancellationToken);
        }

        private async Task ClearState(TeamsAIssistantState turnState)
        {
            if (turnState.Files.Count != 0)
            {
                foreach (var file in turnState.Files)
                {
                    await _fileService.DeleteFileAsync(file);
                }
            }

            turnState.DeleteConversationState();
        }

        private Task HandleResetMessageAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            return HandleResetMessage(turnContext, turnState, cancellationToken, true);
        }

        private async Task HandleResetMessage(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken, bool newCard = false)
        {
            await ClearState(turnState);

            await ShowMenuAsync(turnContext, turnState, cancellationToken, newCard);
        }
    }
}
