using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Pexels
{
    public class PexelsVideoPlugin(IConfiguration configuration,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
            : PexelsBasePlugin(configuration, proactiveMessageService, driveRepository, "Videos")
    {
        [Action("Pexels.SearchVideos")]
        [Description("This endpoint enables you to search Pexels for any topic that you would like")]
        [Parameter(name: "query", type: "string", required: true, description: "The search query. Ocean, Tigers, Pears, etc.")]
        [Parameter(name: "page", type: "number", description: "The page number you are requesting", minimum: 1)]
        [Parameter(name: "size", type: "string", description: "Minimum video size. The current supported sizes are: large (4K), medium (Full HD) or small (HD)", enumValues: ["large", "medium", "small"])]
        [Parameter(name: "orientation", type: "string", description: "Desired video orientation", enumValues: ["landscape", "portrait", "square"])]
        [Parameter(name: "locale", type: "string", description: "The locale of the search you are performing", enumValues: ["en-US", "pt-BR", "es-ES", "ca-ES", "de-DE", "it-IT", "fr-FR", "sv-SE", "id-ID", "pl-PL", "ja-JP", "zh-TW", "zh-CN", "ko-KR", "th-TH", "nl-NL", "hu-HU", "vi-VN", "cs-CZ", "da-DK", "fi-FI", "uk-UA", "el-GR", "ro-RO", "nb-NO", "sk-SK", "tr-TR", "ru-RU"])]
        public Task<string> SearchVideos([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchMediaAsync(turnContext, turnState, actionName, parameters, async (query, page, size, orientation, locale) =>
            {
                var items = await client.SearchVideosAsync(
                       query: query,
                       pageSize: 5,
                       page: page,
                       size: size,
                       orientation: orientation,
                       locale: locale
                   );

                return JsonConvert.SerializeObject(items.videos);
            });
        }

        [Action("Pexels.GetPopularVideos")]
        [Description("This endpoint enables you to receive the current popular Pexels videos")]
        [Parameter(name: "page", type: "number", description: "The page number you are requesting", minimum: 1)]
        [Parameter(name: "min_width", type: "number", description: "The minimum width in pixels of the returned videos.")]
        [Parameter(name: "min_height", type: "number", description: "The minimum height in pixels of the returned videos")]
        [Parameter(name: "min_duration", type: "number", description: "The minimum duration in seconds of the returned videos")]
        [Parameter(name: "max_duration", type: "number", description: "The maximum duration in seconds of the returned videos")]
        public async Task<string> GetPopularVideos([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var page = parameters.TryGetValue("page", out object? value) ? int.Parse(value.ToString()!) : 1;
            var max_duration = parameters.TryGetValue("max_duration", out object? max_durationValue) ? int.Parse(max_durationValue.ToString()!) : int.MaxValue;
            var min_duration = parameters.TryGetValue("min_duration", out object? min_durationValue) ? int.Parse(min_durationValue.ToString()!) : 0;
            var min_height = parameters.TryGetValue("min_height", out object? min_heightValue) ? int.Parse(min_heightValue.ToString()!) : 0;
            var min_width = parameters.TryGetValue("min_width", out object? min_widthValue) ? int.Parse(min_widthValue.ToString()!) : 0;

            try
            {
                var result = await client.PopularVideosAsync(page: page,
                    minDurationSecs: min_duration,
                    maxDurationSecs: max_duration,
                    minHeight: min_height,
                    pageSize: DefaultPageSize,
                    minWidth: min_width);

                var json = JsonConvert.SerializeObject(result.videos);
                
                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, json, cardId);

                return json;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
