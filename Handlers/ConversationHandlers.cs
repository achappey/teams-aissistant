﻿using System.Globalization;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.AdaptiveCards;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    public class ConversationHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly PluginService _pluginService;
        private readonly FileService _fileService;
        private readonly ProactiveMessageService _proactiveMessageService;
        public readonly ActionSubmitHandler<TeamsAIssistantState> UpdateConversationHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> ResetConversationHandler;
        public readonly RouteHandler<TeamsAIssistantState> HandleResetMessageHandler;
        public readonly RouteHandler<TeamsAIssistantState> MenuHandler;
        public readonly UserService? _userService;

        public ConversationHandlers(AssistantService assistantService,
            ProactiveMessageService proactiveMessageService, FileService fileService,
            PluginService pluginService, UserService? userService = null)
        {
            _assistantService = assistantService;
            _fileService = fileService;
            _userService = userService;
            _pluginService = pluginService;
            _proactiveMessageService = proactiveMessageService;

            HandleResetMessageHandler = HandleResetMessageAsync;
            UpdateConversationHandler = HandleUpdateConversationAsync;
            MenuHandler = HandleMenuAsync;
            ResetConversationHandler = HandleResetMessageAsync;
        }

        public Task HandleMenuAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            CancellationToken cancellationToken)
        {
            return ShowMenuAsync(turnContext, turnState, cancellationToken, true);
        }

        private async Task ShowMenuAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            CancellationToken cancellationToken,
            bool newCard = false)
        {
            var fetchAssistantsTask = FetchAssistants(turnContext, turnState);

            Task<IEnumerable<Models.Message>> messagesTask = Task.FromResult<IEnumerable<Models.Message>>([]);
            Task<IEnumerable<(string model, int input, int output)>> threadUsageTask = Task.FromResult<IEnumerable<(string model, int input, int output)>>([]);

            if (turnState.ThreadId != null)
            {
                messagesTask = _assistantService.GetThreadMessagesAsync(turnState.ThreadId);
                threadUsageTask = _assistantService.GetThreadUsageAsync(turnState.ThreadId);
            }

            await Task.WhenAll(fetchAssistantsTask, messagesTask, threadUsageTask);
            
            var (assistant, assistants) = await fetchAssistantsTask;
            var messages = await messagesTask;
            var threadUsageTotals = await threadUsageTask;

            var tools = turnState.Tools.Count != 0 ? turnState.Tools.Where(a => !a.Value.IsFunctionTool()).Select(t => t.Key)
                : assistant.Tools.GetNonFunctionTools().Select(AssistantExtensions.ToToolIdentifier);
                
            var visibleTools = tools.Where(r => r != Tool.FUNCTION_CALLING_TYPE);

            var allPlugins = _pluginService.GetPlugins() ?? [];
            IEnumerable<string> assistantPlugins = assistant.GetPlugins().ToStringList() ?? [];

            double? usage = threadUsageTotals.Select(a => AIPricing.CalculateCost(a.model, a.input, a.output)).Sum();

            var filterCount = turnState.YearFilters.Count + turnState.TypeFilters.Count;
            var sourceCount = turnState.SiteIndexes.Count + turnState.TeamIndexes.Count + turnState.SimplicateIndexes.Count;

            MenuCardData menuCard = new(new CultureInfo(turnContext.Activity.Locale))
            {
                Assistant = assistant,
                IsAuthenticated = turnState.IsAuthenticated(),
                AssistantPlugins = assistantPlugins,
                Assistants = assistants?.Select(t => new AdaptiveChoice() { Value = t.Id, Title = t.Name! }),
                AdditionalInstructions = turnState.AdditionalInstructions ?? string.Empty,
                Tools = tools,
                SelectedSourcesCount = sourceCount > 0 ? sourceCount : null,
                Usage = (usage ?? 0).ToString("C", CultureInfo.CreateSpecificCulture("en-US")),
                ExportToolCalls = turnState.ExportToolCalls ?? false,
                AllPlugins = allPlugins.Select(t => t.Name),
                ConversationPlugins = turnState.Plugins,
                FileCount = assistant.FileIds.Count + turnState.Files.Count,
                PrependDateTime = turnState.PrependDateTime ?? false,
                PrependUsername = turnState.PrependUsername ?? false,
                Model = turnState.GetModel(assistant),
                MessageCount = messages.Count(),
                BotName = turnContext.Activity.Recipient.Name,
            };

            await _proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                               () => MenuCard.MenuCardTemplate.RenderAdaptiveCard(menuCard),
                                newCard ? null : turnContext.Activity.ReplyToId, cancellationToken);
        }

        private async Task<(Assistant assistant, IEnumerable<Assistant>? assistants)> FetchAssistants(
            ITurnContext turnContext,
            TeamsAIssistantState turnState)
        {
            // Start de taak om de primaire assistent te halen
            var assistantTask = _assistantService.GetAssistantAsync(turnState.AssistantId);

            // Voorbereiden van een taak voor het ophalen van assistenten, maar start deze alleen indien nodig
            Task<IEnumerable<Assistant>>? assistantsTask = null;

            if (turnContext.Activity.From.AadObjectId != null)
            {
                assistantsTask = _assistantService.GetAssistantsAsync(turnContext.Activity.From.AadObjectId);
            }

            // Wacht op de assistent taak om te voltooien omdat deze altijd nodig is
            var assistant = await assistantTask;

            // Als de assistantsTask gestart is, wacht dan op het resultaat; anders, stel assistants in op null
            var assistants = assistantsTask != null ? await assistantsTask : null;

            return (assistant, assistants);
        }

        private async Task HandleUpdateConversationAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            object data,
            CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            UpdateTurnStateFromJObject(jObject, turnState);

            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);
            var newTools = jObject[AssistantForm.Tools]?.Value<string>()?.ToStringList();

            var updatedSites = UpdateSiteIndexes([], assistant.GetSiteIndexes() ?? [], turnState.SiteIndexes);
            turnState.SiteIndexes = updatedSites.ToList();
            turnState.DriveIndexes = assistant.GetDriveIndexes()?.ToList() ?? [];

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

        private static IEnumerable<string> UpdateSiteIndexes(IEnumerable<string> newIndexes,
            IEnumerable<string> assistantSites,
            IEnumerable<string> currentSites)
        {
            var sites = newIndexes.ToList();
            sites.AddRange(assistantSites);
            sites.AddRange(currentSites);
            return sites.Distinct();
        }

        private static Dictionary<string, Tool> UpdateTools(IEnumerable<string> newTools,
            IEnumerable<Tool> assistantTools,
            Dictionary<string,
            Tool> currentTools)
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

        private Task HandleResetMessageAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            object data,
            CancellationToken cancellationToken)
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

        private Task HandleResetMessageAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            CancellationToken cancellationToken)
        {
            return HandleResetMessage(turnContext, turnState, cancellationToken, true);
        }

        private async Task HandleResetMessage(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            CancellationToken cancellationToken,
            bool newCard = false)
        {
            await ClearState(turnState);

            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);

            var updatedSites = UpdateSiteIndexes([], assistant.GetSiteIndexes() ?? [], turnState.SiteIndexes);
            turnState.SiteIndexes = updatedSites.ToList();
            turnState.DriveIndexes = assistant.GetDriveIndexes()?.ToList() ?? [];

            var updatedTools = UpdateTools([], assistant.Tools, turnState.Tools);
            turnState.Tools = updatedTools;

            // turnState.SaveStateAsync(turnContext, this._st)
            await ShowMenuAsync(turnContext, turnState, cancellationToken, newCard);
        }
    }
}
