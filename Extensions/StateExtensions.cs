using System.Globalization;
using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Models;
using TeamsAIssistant.State;

namespace TeamsAIssistant.Extensions
{
    public static class StateExtensions
    {
        public static bool IsAuthenticated(this TeamsAIssistantState state)
        {
            return state.Temp.AuthTokens.ContainsKey(Auth.Graph);
        }

        public static bool HasIndexes(this TeamsAIssistantState state)
        {
            return state.HasSiteIndexes() || state.HasTeamIndexes() || state.HasSimplicateIndexes() || state.HasDriveIndexes();
        }

        public static bool HasDriveIndexes(this TeamsAIssistantState state)
        {
            return state.DriveIndexes.Count != 0;
        }

        public static bool HasSiteIndexes(this TeamsAIssistantState state)
        {
            return state.SiteIndexes.Count != 0;
        }

        public static bool HasSimplicateIndexes(this TeamsAIssistantState state)
        {
            return state.SimplicateIndexes.Count != 0;
        }

        public static bool HasTeamIndexes(this TeamsAIssistantState state)
        {
            return state.TeamIndexes.Count != 0;
        }

        public static string GetModel(this TeamsAIssistantState state, Assistant assistant)
        {
            return !string.IsNullOrEmpty(state.Model) ? state.Model : assistant.Model;
        }

        public static string? GetGraphToken(this TeamsAIssistantState state)
        {
            return state.Temp.AuthTokens.TryGetValue(Auth.Graph, out string? value) ? value : null;
        }

        public static void EnsureTool(this TeamsAIssistantState turnState, Tool tool, IEnumerable<Tool> assistantTools)
        {
            Dictionary<string, Tool> tools = turnState.Tools.Count != 0 ? turnState.Tools : assistantTools.ToDictionary(t => t.ToToolIdentifier(), t => t);

            switch (tool.Type)
            {
                case Tool.CODE_INTERPRETER_TYPE:
                    if (!tools.ContainsKey(Tool.CODE_INTERPRETER_TYPE))
                    {
                        tools[Tool.CODE_INTERPRETER_TYPE] = Tool.CODE_INTERPRETER_TYPE.GetToolFromType();

                        tools.Remove(OpenAI.Tool.Retrieval.Type);
                        turnState.Tools = tools;
                    }
                    break;
                case "retrieval":
                    if (!tools.ContainsKey(Tool.CODE_INTERPRETER_TYPE) && !tools.ContainsKey(OpenAI.Tool.Retrieval.Type))
                    {
                        tools[OpenAI.Tool.Retrieval.Type] = OpenAI.Tool.Retrieval.Type.GetToolFromType();
                        turnState.Tools = tools;
                    }
                    break;
            }
        }

        public static void AddPlugin(this TeamsAIssistantState turnState, Plugin plugin, IEnumerable<Tool> assistantTools)
        {
            if (turnState.Tools.Count != 0)
            {
                foreach (PluginAction tool in plugin.Actions ?? [])
                {
                    if (tool.Tool != null)
                    {
                        turnState.Tools[tool.Tool.ToToolIdentifier()] = tool.Tool;
                    }
                }
            }
            else
            {
                foreach (Tool tool in assistantTools ?? [])
                {
                    turnState.Tools[tool.ToToolIdentifier()] = tool;
                }

                foreach (PluginAction tool in plugin.Actions ?? [])
                {
                    if (tool.Tool != null)
                    {
                        turnState.Tools[tool.Tool.ToToolIdentifier()] = tool.Tool;
                    }
                }
            }

            turnState.Plugins.Add(plugin.Name);
        }

        public static void DeletePlugin(this TeamsAIssistantState turnState, Plugin plugin)
        {
            if (turnState.Tools.Count != 0)
            {
                foreach (PluginAction tool in plugin.Actions ?? [])
                {
                    if (tool.Tool != null)
                    {
                        turnState.Tools.Remove(tool.Tool.ToToolIdentifier());
                    }
                }

                turnState.Plugins.Remove(plugin.Name);
            }

        }

        public static string? GetAdditionalInstructions(this ITurnContext turnContext, TeamsAIssistantState turnState)
        {
            var stringBuilder = new StringBuilder();

            if (turnState.PrependDateTime == true && turnContext.Activity.LocalTimestamp is DateTimeOffset localTimestamp)
            {
                string formattedDate = localTimestamp.ToString("F", new CultureInfo(turnContext.Activity.Locale));
                stringBuilder.AppendLine($"LocalTime {formattedDate} Locale {turnContext.Activity.Locale} LocalTimezone {turnContext.Activity.LocalTimezone}");
            }

            if (turnState.PrependUsername == true && !string.IsNullOrEmpty(turnContext.Activity.From.Name))
            {
                stringBuilder.AppendLine($"User {turnContext.Activity.From.Name} AadObjectId {turnContext.Activity.From.AadObjectId}");
            }

            if (!string.IsNullOrEmpty(turnState.AdditionalInstructions))
            {
                stringBuilder.AppendLine(turnState.AdditionalInstructions);
            }

            return stringBuilder.ToString();
        }
    }
}