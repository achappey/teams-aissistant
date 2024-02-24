using Microsoft.Bot.Builder;
using Microsoft.KernelMemory;
using Microsoft.Teams.AI.AI;
using Microsoft.Teams.AI.AI.Planners;
using Microsoft.Teams.AI.AI.Planners.Experimental;
using Microsoft.Teams.AI.AI.Prompts.Sections;
using TeamsAIssistant.DataSources;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Planner
{
    public class TeamsAIssistantPlanner(
        ActionPlannerOptions<TeamsAIssistantState> actionPlannerOptions,
        KernelMemoryData kernelMemoryData,
        AssistantService assistantService,
        AssistantsPlannerOptions assistantsPlannerOptions, ILoggerFactory logger) : IPlanner<TeamsAIssistantState>
    {
        private readonly ActionPlanner<TeamsAIssistantState> actionPlanner = new(actionPlannerOptions, logger);
        private readonly AssistantsPlanner<TeamsAIssistantState> assistantsPlanner = new(assistantsPlannerOptions, logger);

        public async Task<Plan> BeginTaskAsync(ITurnContext turnContext, TeamsAIssistantState turnState,
            AI<TeamsAIssistantState> ai, CancellationToken cancellationToken = default)
        {
            var citations = await EnsureContext(turnContext, turnState, cancellationToken)!;
            var plan = await assistantsPlanner.BeginTaskAsync(turnContext, turnState, ai, cancellationToken);

            return plan.AddCitations(citations, turnState.MaxCitations);
        }

        public async Task<Plan> ContinueTaskAsync(ITurnContext turnContext, TeamsAIssistantState turnState,
            AI<TeamsAIssistantState> ai, CancellationToken cancellationToken = default)
        {
            var citations = await EnsureContext(turnContext, turnState, cancellationToken)!;
            var plan = await assistantsPlanner.ContinueTaskAsync(turnContext, turnState, ai, cancellationToken);

            return plan.AddCitations(citations, turnState.MaxCitations);
        }

        private async Task<IEnumerable<Citation>?>? EnsureContext(ITurnContext turnContext,
            TeamsAIssistantState turnState,
            CancellationToken cancellationToken = default)
        {
            if (turnState.HasIndexes())
            {
                var query = await CreateContextQuery(turnContext, turnState, cancellationToken);

                var (context, citations) = await kernelMemoryData.RenderDataAsync(query,
                    turnState,
                    turnContext,
                    actionPlanner.Options.Tokenizer,
                    turnState.ContextLength ?? Constants.AIConstants.DefaultContextTokenLength);

                var contextString = $"Context:\n{context}";

                if (turnState.AdditionalInstructionsContext == true)
                {
                    turnState.Temp.AdditionalInstructions += $"\n\n{contextString}";
                }
                else
                {
                    turnState.Temp.Input = $"{contextString}\n\nUser: {turnState.Temp.Input}";
                }

                return citations;
            }

            return null;
        }

        private async Task<string> CreateContextQuery(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken = default)
        {
            var query = $"{turnState.AdditionalInstructions}\n\n{turnState.Temp.Input}";

            if (turnState.ThreadId != null)
            {
                var lastMessages = await assistantService.GetLastMessages(turnState.ThreadId, 10);
                var messages = lastMessages.Reverse();
                var promptTemplate = actionPlannerOptions.Prompts.GetPrompt("Chat");

                string messageHistory = string.Join("\n\n", messages?.Select(t => t.ToEmbeddingsSearch())!);
                promptTemplate.Prompt.Sections.Insert(0, new TextSection(text: $"Previous Messages:\n###\n{messageHistory}\n###",
                    role: Microsoft.Teams.AI.AI.Models.ChatRole.User));

                promptTemplate.Prompt.Sections.Insert(1, new TextSection(text: $"Current Message:\n###\n{turnState.Temp.Input}\n###",
                    role: Microsoft.Teams.AI.AI.Models.ChatRole.User));

                var result = await actionPlanner.CompletePromptAsync(turnContext, turnState, promptTemplate, null, cancellationToken);
                query = result.Message?.Content ?? query;

            }

            return query;
        }
    }
}
