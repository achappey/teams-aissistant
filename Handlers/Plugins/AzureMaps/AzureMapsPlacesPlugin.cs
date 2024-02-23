using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Azure.Core.GeoJson;
using Newtonsoft.Json;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Handlers.Plugins.AzureMaps.Extensions;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.AzureMaps
{
    public class AzureMapsPlacesPlugin(IConfiguration configuration, TeamsAdapter teamsAdapter,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
                : AzureMapsBasePlugin(configuration, teamsAdapter, proactiveMessageService, driveRepository, "Azure Maps Places")
    {

        [Action("AzureMaps.GetRouteDirections")]
        [Description("Calculate and retrieve route directions between two points")]
        [Parameter(name: "startLatitude", type: "number", required: true, description: "Latitude of the route start")]
        [Parameter(name: "startLongitude", type: "number", required: true, description: "Longitude of the route start")]
        [Parameter(name: "endLatitude", type: "number", required: true, description: "Latitude of the route end")]
        [Parameter(name: "endLongitude", type: "number", required: true, description: "Longitude of the route end")]
        [Parameter(name: "travelMode", type: "string", required: true, description: "Travel mode",
            enumValues: ["car", "bicycle", "motorcycle", "van", "pedestrian", "bus", "taxi", "truck"])]
        public async Task<string> GetRouteDirections([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var startLatitude = parameters["startLatitude"]?.ToString();
            if (string.IsNullOrEmpty(startLatitude))
            {
                return "Start latitude missing";
            }

            var startLongitude = parameters["startLongitude"]?.ToString();
            if (string.IsNullOrEmpty(startLongitude))
            {
                return "Start longitude missing";
            }

            var endLatitude = parameters["endLatitude"]?.ToString();
            if (string.IsNullOrEmpty(endLatitude))
            {
                return "End latitude missing";
            }

            var endLongitude = parameters["endLongitude"]?.ToString();
            if (string.IsNullOrEmpty(endLongitude))
            {
                return "End longitude missing";
            }

            return await ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var routeResult = await _mapsRouteClient.GetDirectionsAsync(
                            new([
                                new(double.Parse(startLongitude), double.Parse(startLatitude)),
                                new(double.Parse(endLongitude), double.Parse(endLatitude))],
                                new()
                                {
                                    TravelMode = new(parameters.TryGetValue("travelMode", out var travelModeValue)
                                        ? travelModeValue?.ToString() ?? string.Empty : TravelMode.Car.ToString()),
                                    Language = parameters.TryGetValue("language", out object? value) ? value?.ToString() : RoutingLanguage.EnglishUsa,
                                }));

                        return JsonConvert.SerializeObject(routeResult.Value.Routes);
                    });
        }

        [Action("AzureMaps.SearchAddresses")]
        [Description("Search addresses and POIs by query and location")]
        [Parameter(name: "query", type: "string", required: true, description: "Search query")]
        [Parameter(name: "latitude", type: "number", required: true, description: "Latitude of the search")]
        [Parameter(name: "longitude", type: "number", required: true, description: "Longitude of the search")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> SearchAddresses([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteAzureMapsQuery(
                turnContext, turnState, actionName, parameters,
                async () =>
                    {
                        var (latitude, longitude) = parameters.GetLatLong();

                        var searchResult = await _mapsSearchClient.FuzzySearchAsync(
                        parameters["query"]?.ToString(), new()
                        {
                            Coordinates = new GeoPosition(double.Parse(longitude), double.Parse(latitude)),
                            Language = parameters.TryGetValue("language", out object? value) ? value?.ToString() : SearchLanguage.EnglishUsa,
                            Top = parameters.GetTop(),
                            Skip = parameters.GetSkip()
                        });

                        return JsonConvert.SerializeObject(searchResult.Value?.Results, new JsonSerializerSettings()
                        {
                            Error = (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
                            {
                                args.ErrorContext.Handled = true;
                            }
                        });
                    });
        }
    }
}
