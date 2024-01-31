using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.AzureMaps
{
    public class AzureMapsWeatherPlugin(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : AzureMapsBasePlugin(configuration, httpClientFactory, proactiveMessageService, driveRepository, "Azure Maps Weather")
    {
        private const string WEATHER_BASE_URL = "weather";

        [Action("AzureMaps.GetSevereWeatherAlerts")]
        [Description("Gets severe weather alerts")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the location")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the location")]
        public Task<string> GetSevereWeatherAlerts([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                   [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();
                        using var response = await _httpClient.GetAsync($"{WEATHER_BASE_URL}/severe/alerts/json?api-version=1.0&query={latitude},{longitude}&details=true");

                        return await response.GetHttpResponseResult();
                    });
        }

        [Action("AzureMaps.GetCurrentWeatherConditions")]
        [Description("Gets the current weather conditions at a location")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the location")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the location")]
        public Task<string> GetCurrentConditions([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();
                        using var response = await _httpClient.GetAsync($"{WEATHER_BASE_URL}/currentConditions/json?api-version=1.0&query={latitude},{longitude}");

                        return await response.GetHttpResponseResult();
                    });
        }

        [Action("AzureMaps.GetWeatherForecast")]
        [Description("Gets the current weather conditions at a location")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the location")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the location")]
        [Parameter(name: "duration", type: "string", enumValues: ["1", "5", "10"], description: "Specifies for how many days the daily forecast responses are returned")]
        public Task<string> GetWeatherForecast([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();
                        var filterString = parameters.ToMapsFilterString(["latitude", "longitude"]);
                        using var response = await _httpClient.GetAsync($"{WEATHER_BASE_URL}/forecast/daily/json?api-version=1.0&query={latitude},{longitude}&{filterString}");

                        return await response.GetHttpResponseResult();
                    });
        }

        [Action("AzureMaps.GetWeatherHistoricalNormals")]
        [Description("Gets climatology data such as past daily normal temperatures, precipitation and cooling/heating degree day information for the day at a given coordinate location. The date range supported is 1 to 31 calendar days, so be sure to specify a startDate and endDate that does not exceed a maximum of 31 days")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the location")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the location")]
        [Parameter(name: "startDate", type: "string", required: true, description: "Start date in ISO 8601 format, for example, 2019-10-27")]
        [Parameter(name: "endDate", type: "string", required: true, description: "End date in ISO 8601 format, for example, 2019-10-28")]
        public Task<string> GetWeatherHistoricalNormals([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();
                        var filterString = parameters.ToMapsFilterString(["latitude", "longitude"]);
                        using var response = await _httpClient.GetAsync($"{WEATHER_BASE_URL}/forecast/daily/json?api-version=1.0&query={latitude},{longitude}&{filterString}");

                        return await response.GetHttpResponseResult();
                    });
        }


    }
}
