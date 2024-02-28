using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Graph.Beta.Models;
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
    public class AssistantHandlers
    {
        private readonly AssistantService _assistantService;
        private readonly PluginService _pluginService;
        private readonly UserService? _userService;
        private readonly IStorage _storage;
        private readonly GraphClientServiceProvider _graphClientServiceProvider;
        private readonly ProactiveMessageService _proactiveMessageService;
        public RouteHandler<TeamsAIssistantState> AssistantMessageHandler;
        public ActionSubmitHandler<TeamsAIssistantState> UpdateAssistantHandler;
        public ActionSubmitHandler<TeamsAIssistantState> ShowAssistantHandler;
        public ActionSubmitHandler<TeamsAIssistantState> DeleteAssistantHandler;
        public ActionSubmitHandler<TeamsAIssistantState> CloneAssistantHandler;

        public AssistantHandlers(AssistantService assistantService, IStorage storage,
                            ProactiveMessageService proactiveMessageService, PluginService pluginService,
                            GraphClientServiceProvider graphClientServiceProvider)
        {
            _graphClientServiceProvider = graphClientServiceProvider;
            _assistantService = assistantService;
            _proactiveMessageService = proactiveMessageService;
            _storage = storage;
            _pluginService = pluginService;

            AssistantMessageHandler = HandleAssistantMessageAsync;
            UpdateAssistantHandler = HandleUpdateAssistantAsync;
            ShowAssistantHandler = HandleShowAssistantAsync;
            DeleteAssistantHandler = HandleDeleteAssistantAsync;
            CloneAssistantHandler = HandleCloneAssistantAsync;
        }

        public AssistantHandlers(AssistantService assistantService, IStorage storage, UserService userService,
            ProactiveMessageService proactiveMessageService, PluginService pluginService,
            GraphClientServiceProvider graphClientServiceProvider)
        {
            _graphClientServiceProvider = graphClientServiceProvider;
            _assistantService = assistantService;
            _proactiveMessageService = proactiveMessageService;
            _storage = storage;
            _userService = userService;
            _pluginService = pluginService;

            AssistantMessageHandler = HandleAssistantMessageAsync;
            UpdateAssistantHandler = HandleUpdateAssistantAsync;
            ShowAssistantHandler = HandleShowAssistantAsync;
            CloneAssistantHandler = HandleCloneAssistantAsync;
            DeleteAssistantHandler = HandleDeleteAssistantAsync;
        }

        private async Task HandleDeleteAssistantAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            var newAssistant = jObject[AssistantForm.DeleteAssistantId]?.Value<string>();

            if (newAssistant != null)
            {
                var assistant = await _assistantService.GetAssistantAsync(newAssistant);

                if (assistant.IsOwner(_graphClientServiceProvider.AadObjectId!) && !_assistantService.IsDefaultAssistant(newAssistant))
                {
                    var result = await _assistantService.DeleteAssistantAsync(newAssistant);

                    if (result)
                    {
                        turnState.AssistantId = string.Empty;
                        turnState.Model = string.Empty;
                        await turnState.SaveStateAsync(turnContext, _storage);
                    }
                }

                await ShowAssistantCardAsync(turnContext, turnState, cancellationToken, false);
            }
        }

        private Task<Assistant> FetchAssistant(TeamsAIssistantState turnState)
        {
            return _assistantService.GetAssistantAsync(turnState.AssistantId);
        }


        private Task HandleAssistantMessageAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken)
        {
            return ShowAssistantCardAsync(turnContext, turnState, cancellationToken, true);
        }

        private async Task HandleCloneAssistantAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            var newAssistant = jObject[AssistantForm.AssistantId]?.Value<string>();

            if (newAssistant != null)
            {
                var assistant = await _assistantService.CloneAssistantAsync(_graphClientServiceProvider.AadObjectId!, newAssistant);
                turnState.AssistantId = assistant.Id;
                turnState.Model = assistant.Model;
                await turnState.SaveStateAsync(turnContext, _storage);


                await ShowAssistantCardAsync(turnContext, turnState, cancellationToken, true);
            }
        }

        private Task HandleShowAssistantAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return HandleAssistantMessageAsync(turnContext, turnState, cancellationToken);
        }

        private async Task HandleUpdateAssistantAsync(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            var jObject = JObject.FromObject(data);
            var newModel = jObject[AssistantForm.ModelId]?.Value<string>();
            var newName = jObject[AssistantForm.NameId]?.Value<string>();
            var newDescription = jObject[AssistantForm.DescriptionId]?.Value<string>();
            var newInstructions = jObject[AssistantForm.InstructionId]?.Value<string>();
            var metadataString = jObject[AssistantForm.MetadataId]?.ToString();
            var assistantId = jObject[AssistantForm.AssistantId]?.ToString();
            var visibilityString = jObject[AssistantForm.Visibility]?.ToString();
            var teamId = jObject[AssistantForm.Team]?.ToString();
            var newTools = jObject[AssistantForm.Tools]?.Value<string>()?.ToStringList();
            var newPlugins = jObject[AssistantForm.Plugins]?.Value<string>()?.ToStringList();

            List<Tool> pluginTools = [];

            foreach (var plugin in newPlugins ?? [])
            {
                pluginTools.AddRange(_pluginService.GetPluginTools(plugin));
            }

            var tools = newTools?.Select(AssistantExtensions.GetToolFromType).ToList() ?? [];
            tools.AddRange(pluginTools);

            if (metadataString != null)
            {
                var newMetadata = JObject.Parse(metadataString)
                    .ToObject<Dictionary<string, object>>()?
                    .WithVisibility(visibilityString)
                    .WithTeam(teamId)
                    .WithPlugins(string.Join(",", newPlugins ?? []));

                var assistant = new Assistant()
                {
                    Id = assistantId!,
                    Name = newName,
                    Model = newModel!,
                    Tools = tools,
                    Description = newDescription,
                    Instructions = newInstructions,
                    Metadata = newMetadata?.ToDictionary(a => a.Key, a => a.Value),
                };

                await _assistantService.UpdateAssistantAsync(assistant);
                turnState.Model = newModel;

                if (turnState.Plugins.Count != 0)
                {
                    foreach (var plugin in turnState.Plugins)
                    {
                        tools.AddRange(_pluginService.GetPluginTools(plugin));
                    }

                    turnState.Tools = tools.ToDictionary(g => g.ToToolIdentifier(), g => g);
                }
                else
                {
                    turnState.Tools = [];
                }

                await turnState.SaveStateAsync(turnContext, _storage);
            }

            await ShowAssistantCardAsync(turnContext, turnState, cancellationToken, false);
        }

        private async Task ShowAssistantCardAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken, bool newCard = false)
        {
            var assistant = await FetchAssistant(turnState);

            IEnumerable<string>? owners = null;
            IEnumerable<Team>? teams = null;
            string? teamName = null;

            if (turnState.IsAuthenticated() && assistant.HasOwners())
            {
                var ownerValues = assistant.GetMetadataValue(AssistantMetadata.Owners)?.Split(',') ?? [];
                owners = await _userService?.GetUsersByIds(ownerValues)!;
            }

            if (turnState.IsAuthenticated() && assistant.HasTeam())
            {
                var teamValue = assistant.GetMetadataValue(AssistantMetadata.Team)?.ToString();

                if (teamValue != null && _userService != null)
                {
                    var team = await _userService.GetTeam(teamValue);
                    teamName = team?.DisplayName ?? string.Empty;
                }
            }

            if (turnState.IsAuthenticated() && assistant.IsOwner(_graphClientServiceProvider.AadObjectId!))
            {
                teams = await _userService?.GetJoinedTeams(_graphClientServiceProvider.AadObjectId!)!;
                teams = teams.Take(100);
            }

            var canDelete = assistant.IsOwner(_graphClientServiceProvider.AadObjectId!)
                && !string.IsNullOrEmpty(turnState.AssistantId)
                && !_assistantService.IsDefaultAssistant(turnState.AssistantId);

            var allPlugins = _pluginService.GetPluginNames() ?? [];

            AssistantCardData assistantCardData = new(new(turnContext.Activity.Locale))
            {
                Assistant = assistant,
                CanDelete = canDelete,
                TeamName = teamName,
                OwnerNames = owners != null && owners.Any() ? string.Join(", ", owners) : null,
                IsOwner = assistant.IsOwner(_graphClientServiceProvider.AadObjectId!),
                TeamChoices = teams?.Select(t => new AdaptiveChoice() { Title = t.DisplayName, Value = t.Id }) ?? [],
                PluginChoices = allPlugins.Select(CardExtensions.ToAdaptiveChoice),
                IsAuthenticated = turnState.IsAuthenticated(),
            };

            await _proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                                                  () => AssistantCard.AssistantCardTemplate.RenderAdaptiveCard(assistantCardData),
                                                  newCard ? null : turnContext.Activity.ReplyToId, cancellationToken);

        }

    }
}
