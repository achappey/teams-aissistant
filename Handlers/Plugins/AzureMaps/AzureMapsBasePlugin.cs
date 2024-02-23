using Microsoft.Bot.Builder;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using Azure.Maps.Routing;
using Azure.Maps.Search;
using TeamsAIssistant.Config;
using Azure;
using TeamsAIssistant.Extensions;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.AzureMaps
{
    public abstract class AzureMapsBasePlugin : PluginBase
    {
        protected readonly MapsSearchClient _mapsSearchClient;
        protected readonly MapsRoutingClient _mapsRouteClient;
        protected readonly HttpClient _httpClient;

        public AzureMapsBasePlugin(IConfiguration configuration, TeamsAdapter teamsAdapter,
                ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name)
                : base(driveRepository, proactiveMessageService, name, "Microsoft", "Azure Maps API", "v1")
        {
            var configKey = configuration.Get<ConfigOptions>()?.AzureMapsSubscriptionKey;
            var credential = new AzureKeyCredential(configKey ?? string.Empty);
            _mapsSearchClient = new MapsSearchClient(credential);
            _mapsRouteClient = new MapsRoutingClient(credential);
            
            _httpClient = teamsAdapter.GetDefaultClient("https://atlas.microsoft.com/", "AzureMaps");
            _httpClient.DefaultRequestHeaders.Add("Subscription-Key", configKey);
        }

        public async Task<string> ExecuteAzureMapsQuery(
            TurnContext turnContext, TeamsAIssistantState turnState, string actionName,
            Dictionary<string, object> parameters,
            Func<Task<string>> query)
        {
            var cardId = await SendFunctionCard(turnContext, actionName, parameters);

            var missingParams = VerifyParameters(actionName, parameters);
            if (missingParams != null)
            {
                return missingParams;
            }

            var result = await query();

            await UpdateFunctionCard(turnContext, turnState, actionName, parameters, result, cardId);

            return result;
        }

    }
}
