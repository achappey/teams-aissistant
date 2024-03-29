﻿using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Pexels
{
    public class PexelsImagePlugin(IConfiguration configuration,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
            : PexelsBasePlugin(configuration, proactiveMessageService, driveRepository, "Images")
    {
        [Action("Pexels.SearchPhotos")]
        [Description("This endpoint enables you to search Pexels for any topic that you would like")]
        [Parameter(name: "query", type: "string", required: true, description: "The search query. Ocean, Tigers, Pears, etc.")]
        [Parameter(name: "page", type: "number", description: "The page number you are requesting", minimum: 1)]
        [Parameter(name: "size", type: "string", description: "Minimum photo size. The current supported sizes are: large(24MP), medium(12MP) or small(4MP)", enumValues: ["large", "medium", "small"])]
        [Parameter(name: "orientation", type: "string", description: "Desired photo orientation", enumValues: ["landscape", "portrait", "square"])]
        [Parameter(name: "locale", type: "string", description: "The locale of the search you are performing", enumValues: ["en-US", "pt-BR", "es-ES", "ca-ES", "de-DE", "it-IT", "fr-FR", "sv-SE", "id-ID", "pl-PL", "ja-JP", "zh-TW", "zh-CN", "ko-KR", "th-TH", "nl-NL", "hu-HU", "vi-VN", "cs-CZ", "da-DK", "fi-FI", "uk-UA", "el-GR", "ro-RO", "nb-NO", "sk-SK", "tr-TR", "ru-RU"])]
        public Task<string> SearchPhotos([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchMediaAsync(turnContext, turnState, actionName, parameters, async (query, page, size, orientation, locale) =>
            {
                var items = await client.SearchPhotosAsync(
                        query: query,
                        pageSize: DefaultPageSize,
                        page: page,
                        size: size,
                        orientation: orientation,
                        locale: locale
                    );

                return JsonConvert.SerializeObject(items.photos);
            });
        }

        [Action("Pexels.GetCuratedPhotos")]
        [Description("This endpoint enables you to receive real-time photos curated by the Pexels team. We add at least one new photo per hour to our curated list so that you always get a changing selection of trending photos")]
        [Parameter(name: "page", type: "number", description: "The page number you are requesting", minimum: 1)]
        public async Task<string> GetCuratedPhotos([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var page = parameters.TryGetValue("page", out object? value) ? int.Parse(value.ToString()!) : 1;

            try
            {
                var result = await client.CuratedPhotosAsync(page, pageSize: DefaultPageSize);
                var json = JsonConvert.SerializeObject(result.photos);
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
