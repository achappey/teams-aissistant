using System.Text.Json.Nodes;
using Microsoft.Bot.Schema;
using Microsoft.Teams.AI.AI.Models;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using Newtonsoft.Json;
using OpenAI.Assistants;
using OpenAI.Threads;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Extensions
{
    public static class AssistantExtensions
    {
        public static string ToEmbeddingsSearch(this Models.Message message)
        {
            if (message.Role == ChatRole.User && message.Content.IndexOf("\n\nUser: ") > -1)
            {
                return message.Content[(message.Content.IndexOf("\n\nUser: ") + 2)..];
            }

            return $"{message.Role}: {message.Content}";
        }


        public static Models.Message ToMessage(this MessageResponse response)
        {
            return new()
            {
                Id = response.Id,
                CreatedAt = response.CreatedAt,
                Role = response.Role.ToString(),
                Content = response.Content != null && response.Content.Where(r => r.Text != null).Any()
                   ? response.Content.Where(r => r.Text != null)?.FirstOrDefault()?.Text?.Value ?? string.Empty
                     : string.Empty
            };
        }

        public static Dictionary<string, object> WithOwner(this Dictionary<string, object> metadata, string? ownerValue)
        {
            return metadata.WithMetadataValue(AssistantMetadata.Owners, ownerValue);
        }

        public static Dictionary<string, object> WithMetadataValue(this Dictionary<string, object> metadata, string key, string? value)
        {
            if (metadata == null)
            {
                metadata = [];
            }

            if (string.IsNullOrEmpty(value) && !metadata.ContainsKey(key))
            {
                return metadata;
            }

            metadata[key] = value ?? string.Empty;

            return metadata;
        }

        public static Dictionary<string, object> WithPlugins(this Dictionary<string, object> metadata, string? pluginValue)
        {
            return metadata.WithMetadataValue(AssistantMetadata.Plugins, pluginValue);
        }

        public static Dictionary<string, object> WithVisibility(this Dictionary<string, object> metadata, string? visibilityString)
        {
            return metadata.WithMetadataValue(AssistantMetadata.Visibility, visibilityString);
        }

        public static Dictionary<string, object> WithTeam(this Dictionary<string, object> metadata, string? teamId)
        {
            return metadata.WithMetadataValue(AssistantMetadata.Team, teamId);
        }

        public static string? GetVisibility(this Assistant assistant)
        {
            return assistant.GetMetadataValue(AssistantMetadata.Visibility);
        }

        public static IEnumerable<string>? GetSiteIndexes(this Assistant assistant)
        {
            return assistant.GetMetadataValue(AssistantMetadata.Sites)?.ToStringList();
        }

        public static IEnumerable<string>? GetDriveIndexes(this Assistant assistant)
        {
            return assistant.GetMetadataValue(AssistantMetadata.Drives)?.ToStringList();
        }

        public static string? GetMetadataValue(this Assistant assistant, string value)
        {
            return assistant.Metadata != null && assistant.Metadata.ContainsKey(value)
                && assistant.Metadata[value] != null && !string.IsNullOrEmpty(assistant.Metadata[value].ToString())
                ? assistant.Metadata[value].ToString() : null;
        }


        public static string? GetPlugins(this Assistant assistant)
        {
            return assistant.GetMetadataValue(AssistantMetadata.Plugins);
        }

        public static string? GetTeam(this Assistant assistant)
        {
            return assistant.GetMetadataValue(AssistantMetadata.Team);
        }

        public static bool IsOwner(this Assistant assistant, string userId)
        {
            return !string.IsNullOrEmpty(userId) && assistant.Metadata != null && assistant.Metadata.ContainsKey(AssistantMetadata.Owners)
                && assistant.Metadata[AssistantMetadata.Owners] != null
                && assistant.Metadata[AssistantMetadata.Owners].ToString()!.Split(",").Contains(userId);
        }

        public static bool IsTeamMember(this Assistant assistant, string[] teamIds)
        {
            return teamIds.Length > 0 && assistant.Metadata != null && assistant.Metadata.ContainsKey(AssistantMetadata.Team)
                && assistant.Metadata[AssistantMetadata.Team] != null
                && teamIds.Any(e => e == assistant.Metadata[AssistantMetadata.Team].ToString());
        }

        public static bool IsOwner(this Assistant assistant, ChannelAccount channelAccount)
        {
            return assistant.IsOwner(channelAccount.AadObjectId);
        }

        public static bool HasOwners(this Assistant assistant)
        {
            return assistant.Metadata != null && assistant.Metadata.ContainsKey(AssistantMetadata.Owners)
                && assistant.Metadata[AssistantMetadata.Owners] != null
                && !string.IsNullOrEmpty(assistant.Metadata[AssistantMetadata.Owners].ToString());
        }

        public static bool HasTeam(this Assistant assistant)
        {
            return assistant.Metadata != null && assistant.Metadata.ContainsKey(AssistantMetadata.Team)
                && assistant.Metadata[AssistantMetadata.Team] != null
                && !string.IsNullOrEmpty(assistant.Metadata[AssistantMetadata.Team].ToString());
        }

        public static Function ToFunction(this OpenAI.Function function)
        {
            return new Function()
            {
                Name = function.Name,
                Description = function.Description,
                Parameters = function.Parameters?.ToJsonString() != null
                        ? JsonConvert.DeserializeObject<Dictionary<string, object>>(function.Parameters?.ToJsonString()!)!
                        : []
            };
        }

        public static OpenAI.Function ToFunction(this Function function)
        {
            return new OpenAI.Function(name: function.Name,
                description: function.Description,
                parameters: JsonObject.Parse(JsonConvert.SerializeObject(function.Parameters)));
        }

        public static Tool ToTool(this OpenAI.Tool tool)
        {
            return tool.Type switch
            {
                "retrieval" => OpenAI.Tool.Retrieval.Type.GetToolFromType(),
                Tool.CODE_INTERPRETER_TYPE => Tool.CODE_INTERPRETER_TYPE.GetToolFromType(),
                Tool.FUNCTION_CALLING_TYPE => new Tool()
                {
                    Type = Tool.FUNCTION_CALLING_TYPE,
                    Function = tool.Function?.ToFunction()
                },
                _ => throw new ArgumentException("Invalid tool"),
            };
        }

        public static OpenAI.Tool ToTool(this Tool tool)
        {
            return tool.Type switch
            {
                "retrieval" => OpenAI.Tool.Retrieval,
                Tool.CODE_INTERPRETER_TYPE => OpenAI.Tool.CodeInterpreter,
                Tool.FUNCTION_CALLING_TYPE => new OpenAI.Tool(tool.Function?.ToFunction()),
                _ => throw new ArgumentException("Invalid tool"),
            };
        }

        public static bool IsFunctionTool(this Tool tool)
        {
            return tool.Type == Tool.FUNCTION_CALLING_TYPE;
        }

        public static IEnumerable<Tool> GetNonFunctionTools(this IEnumerable<Tool> tools)
        {
            return tools.Where(t => !IsFunctionTool(t));
        }

        public static string ToToolIdentifier(this Tool tool)
        {
            if (tool.Type == Tool.FUNCTION_CALLING_TYPE)
            {
                if (tool.Function == null)
                {
                    throw new ArgumentException("Function missing");
                }

                return tool.Function.Name;
            }

            return tool.Type;
        }

        public static Assistant ToAssistant(this AssistantResponse response)
        {
            return new()
            {
                Id = response.Id,
                Name = response.Name,
                Model = response.Model,
                CreatedAt = response.CreatedAt.ToFileTimeUtc(),
                Description = response.Description,
                FileIds = [.. response.FileIds],
                Metadata = response.Metadata.ToDictionary(t => t.Key, y => (object)y.Value),
                Tools = response.Tools.Select(ToTool).ToList(),
                Instructions = response.Instructions
            };
        }

        public static Tool GetToolFromType(this string type)
        {
            return new()
            {
                Type = type
            };
        }

        public static Tool? GetToolTypeFromFile(this string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();

            if (FileTypes.DataRetrievalExtensions.Contains(extension))
            {
                return OpenAI.Tool.Retrieval.Type.GetToolFromType();
            }
            else if (FileTypes.CodeInterpreterExtensions.Contains(extension))
            {
                return Tool.CODE_INTERPRETER_TYPE.GetToolFromType();
            }

            return null;
        }

    }
}
