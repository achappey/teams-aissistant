using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Extensions;
using Microsoft.Bot.Schema;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Handlers.Plugins.AzureMaps
{
    public class AzureMapsRenderPlugin(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : AzureMapsBasePlugin(configuration, httpClientFactory, proactiveMessageService, driveRepository, "Azure Maps Render")
    {
        private const string MAPS_BASE_URL = "map";

        [Action("AzureMaps.RenderMapImage")]
        [Description("Renders a map image")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the location")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the location")]
        [Parameter(name: "style", type: "string", enumValues: ["dark", "main"], description: "Style of the map")]
        [Parameter(name: "zoom", type: "number", minimum: 0, maximum: 20, description: "Desired zoom level of the map. The higher the number, the more zoomed in the view is")]
        [Parameter(name: "height", type: "number", minimum: 1, maximum: 8192, description: "Height of the resulting image in pixels")]
        [Parameter(name: "width", type: "number", minimum: 1, maximum: 8192, description: "Width of the resulting image in pixels")]
        public Task<string> RenderMapImage([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();
                        var filterString = parameters.ToMapsFilterString(["latitude", "longitude"]);

                        using var response = await _httpClient.GetAsync($"{MAPS_BASE_URL}/static/png?api-version=2022-08-01&{filterString}&center={longitude},{latitude}");

                        if (!response.IsSuccessStatusCode)
                        {
                            return response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage;
                        }

                        var resultData = await response.Content.ReadAsByteArrayAsync();
                        string base64Image = Convert.ToBase64String(resultData);

                        IMessageActivity imageMessage = MessageFactory.Text(null);
                        imageMessage.Attachments =
                            [
                                new() {
                                    ContentType = "image/png",
                                    ContentUrl = $"data:image/png;base64,{base64Image}"
                                }
                            ];

                        await turnContext.SendActivityAsync(imageMessage);

                        return "Maps image generated and presented to the user in an adaptive card.";
                    });
        }
    }
}
