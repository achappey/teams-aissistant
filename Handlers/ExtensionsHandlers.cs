using System.Globalization;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Graph.Beta.Models;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.AdaptiveCards;
using TeamsAIssistant.Config;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Handlers
{
    public class ExtensionsHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly PluginService _pluginService;
        private readonly GraphClientServiceProvider _graphClientServiceProvider;
        private readonly ProactiveMessageService _proactiveMessageService;
        public readonly ActionSubmitHandler<TeamsAIssistantState> UpdatePluginsHandler;
        public readonly ActionSubmitHandler<TeamsAIssistantState> UpdateKernelMemoryHandler;
        public ActionSubmitHandler<TeamsAIssistantState> ShowExtensionsHandler;
        public readonly RouteHandler<TeamsAIssistantState> MenuHandler;
        public readonly UserService _userService;
        public readonly IConfiguration _configuration;

        public ExtensionsHandlers(AssistantService assistantService,
            ProactiveMessageService proactiveMessageService,
            IConfiguration configuration,
            GraphClientServiceProvider graphClientServiceProvider,
            PluginService pluginService, UserService userService)
        {
            _assistantService = assistantService;
            _userService = userService;
            _configuration = configuration;
            _graphClientServiceProvider = graphClientServiceProvider;
            _pluginService = pluginService;
            _proactiveMessageService = proactiveMessageService;

            UpdateKernelMemoryHandler = HandleUpdateKernelMemoryAsync;
            MenuHandler = HandleMenuAsync;
            UpdatePluginsHandler = HandleUpdatePluginsAsync;
            ShowExtensionsHandler = HandleMenuAsync;
        }

        private async Task HandleUpdatePluginsAsync(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            object data,
            CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            turnState.CreateFunctionExports = jObject[AssistantForm.ExportFunctionOutput]?.Value<bool>() ?? false;

            var assistant = await _assistantService.GetAssistantAsync(turnState.AssistantId);
            var newPlugins = jObject[AssistantForm.Plugins]?.Value<string>()?.ToStringList();

            var updatedTools = UpdateToolsWithPlugins(newPlugins ?? [], assistant.Tools, turnState.Tools);
            turnState.Tools = updatedTools;
            turnState.Plugins = newPlugins?.ToList() ?? [];

            await ShowMenuAsync(turnContext, turnState, cancellationToken);
        }

        private async Task HandleUpdateKernelMemoryAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            UpdateTurnStateForKernelMemory(jObject, turnState);

            await ShowMenuAsync(turnContext, turnState, cancellationToken);
        }

        private static void UpdateTurnStateForKernelMemory(JObject jObject, TeamsAIssistantState turnState)
        {
            turnState.AdditionalInstructionsContext = jObject[AssistantForm.AdditionalInstructionsContext]?.Value<bool>() ?? false;
            turnState.SiteIndexes = jObject[AssistantForm.Sites]?.Value<string>()?.ToStringList() ?? [];
            turnState.TeamIndexes = jObject[AssistantForm.Teams]?.Value<string>()?.ToStringList() ?? [];
            turnState.DataverseIndexes = jObject[AssistantForm.Dataverse]?.Value<string>()?.ToStringList() ?? [];
            turnState.GraphIndexes = jObject[AssistantForm.Graph]?.Value<string>()?.ToStringList() ?? [];
            turnState.MaxCitations = jObject[AssistantForm.MaxCitations]?.Value<int?>() ?? -1;
            turnState.ContextLength = jObject[AssistantForm.ContextLength]?.Value<int?>() ?? AIConstants.DefaultContextTokenLength;
            turnState.MinRelevance = jObject[AssistantForm.MinRelevance]?.Value<double?>() ?? AIConstants.DefaultMinRelevance;
            turnState.SimplicateIndexes = jObject[AssistantForm.Simplicate]?.Value<string>()?.ToStringList() ?? [];
            turnState.YearFilters = jObject[AssistantForm.Years]?.Value<string>()?.ToStringList() ?? [];
            turnState.TypeFilters = jObject[AssistantForm.Types]?.Value<string>()?.ToStringList() ?? [];
        }

        private Dictionary<string, Tool> UpdateToolsWithPlugins(IEnumerable<string> newPlugins,
            IEnumerable<Tool> assistantTools,
            Dictionary<string, Tool> currentTools)
        {
            var resultTools = new Dictionary<string, Tool>(currentTools);
            var pluginTools = GetToolsFromPlugins(newPlugins);

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

        public Task HandleMenuAsync(ITurnContext turnContext,
                TeamsAIssistantState turnState,
                object data,
                CancellationToken cancellationToken)
        {
            return ShowMenuAsync(turnContext, turnState, cancellationToken, true);
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
            var assistantTask = _assistantService.GetAssistantAsync(turnState.AssistantId);

            Task<IEnumerable<Site>> currentSitesTask = Task.FromResult<IEnumerable<Site>>([]);
            Task<IEnumerable<Team>> currentTeamsTask = Task.FromResult<IEnumerable<Team>>([]);
            Task<IEnumerable<Team>> joinedTeamsTask = _userService.GetJoinedTeams(_graphClientServiceProvider.AadObjectId!).ContinueWith(task => task.Result.Take(200));
            Task<IEnumerable<Site>> followedSitesTask = _userService.GetFollowedSites(_graphClientServiceProvider.AadObjectId!);

            if (turnState.SiteIndexes.Count > 0)
            {
                currentSitesTask = _userService.GetSites(turnState.SiteIndexes);
            }
            if (turnState.TeamIndexes.Count > 0)
            {
                currentTeamsTask = _userService.GetTeams(turnState.TeamIndexes);
            }

            await Task.WhenAll(assistantTask, currentSitesTask, currentTeamsTask, joinedTeamsTask, followedSitesTask);

            var assistant = await assistantTask;
            var currentSites = await currentSitesTask;
            var currentTeams = await currentTeamsTask;
            var joinedTeams = await joinedTeamsTask;
            var followedSites = await followedSitesTask;

            var tools = turnState.Tools.Count != 0 ? turnState.Tools.Where(a => !a.Value.IsFunctionTool()).Select(t => t.Key)
                : assistant.Tools.GetNonFunctionTools().Select(AssistantExtensions.ToToolIdentifier);
            var visibleTools = tools.Where(r => r != Tool.FUNCTION_CALLING_TYPE);
            var allPlugins = _pluginService.GetPlugins() ?? [];
            IEnumerable<string> assistantPlugins = assistant.GetPlugins().ToStringList() ?? [];

            var selectedItems = currentSites?.Select(g => g.DisplayName!).ToList();
            selectedItems?.AddRange(currentTeams?.Select(g => g.DisplayName!) ?? []);
            selectedItems?.AddRange(turnState.SimplicateIndexes);

            var filterCount = turnState.YearFilters.Count + turnState.TypeFilters.Count;
            var dataverses = _configuration.Get<ConfigOptions>()!.DataverseConnections?.ToStringList()?
                .Select(a => new AdaptiveChoice() { Title = a.Split(";").ElementAt(0), Value = a });

            ExtensionsCardData menuCard = new(new(turnContext.Activity.Locale))
            {
                IsAuthenticated = turnState.IsAuthenticated(),
                AssistantPlugins = assistantPlugins,
                Tools = tools,
                Dataverses = dataverses,
                SelectedGraphSources = string.Join(",", turnState.GraphIndexes),
                SelectedDataverses = string.Join(",", turnState.DataverseIndexes),
                AdditionalInstructionsContext = turnState.AdditionalInstructionsContext ?? false,
                ContextLength = turnState.ContextLength ?? AIConstants.DefaultContextTokenLength,
                MaxCitations = turnState.MaxCitations.HasValue && turnState.MaxCitations >= 0 ? turnState.MaxCitations : null,
                SelectedSimplicateModules = turnState.SimplicateIndexes.ToListString(),
                Sites = followedSites,
                SelectedSources = selectedItems?.ToListString(),
                SelectedYears = turnState.YearFilters.ToListString(),
                SelectedTypes = turnState.TypeFilters.ToListString(),
                FilterCount = filterCount,
                SelectedSites = turnState.SiteIndexes.ToListString(),
                Teams = joinedTeams,
                SelectedTeams = turnState.TeamIndexes.ToListString(),
                AllPlugins = allPlugins.Select(t => t.Name),
                MinRelevance = turnState.MinRelevance.ToString(CultureInfo.InvariantCulture),
                ConversationPlugins = turnState.Plugins,
                ExportFunctionOutput = turnState.CreateFunctionExports ?? false,
            };

            await _proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                               () => ExtensionsCard.ExtensionsCardemplate.RenderAdaptiveCard(menuCard),
                                newCard ? null : turnContext.Activity.ReplyToId, cancellationToken);
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

    }
}
