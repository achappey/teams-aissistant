using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using Newtonsoft.Json;
using Microsoft.Graph.Beta;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json.Linq;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public abstract class GraphBasePlugin(GraphClientServiceProvider graphClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository, string name)
        : PluginBase(driveRepository, proactiveMessageService, name, "Microsoft", "Graph API", "beta")
    {
        protected readonly GraphClientServiceProvider _graphClientServiceProvider = graphClientServiceProvider;

        public async Task<string> ExecuteGraphQuery<T>(
            TurnContext turnContext, TeamsAIssistantState turnState, string actionName,
            Dictionary<string, object> parameters,
            Func<GraphServiceClient, Dictionary<string, object>, Task<T>> query)
        {
            if (!turnState.IsAuthenticated())
            {
                return "Not authenticated";
            }

            var cardId = await SendFunctionCard(turnContext, actionName, parameters);
            var missingParams = VerifyParameters(actionName, parameters);

            if (missingParams != null)
            {
                return missingParams;
            }

            var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();

            try
            {
                var result = await query(graphClient, parameters);
                var json = JsonConvert.SerializeObject(result);

                await UpdateFunctionCard(turnContext, turnState, actionName, parameters, json, cardId);

                return json;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        protected async Task SubmitActionAsync(
            ITurnContext turnContext,
            TeamsAIssistantState turnState,
            string actionName,
            object data,
            Func<GraphServiceClient, JObject?, Task<string>> actionMethod,
            CancellationToken cancellationToken)
        {
            JObject jObject = JObject.FromObject(data);
            var parametersDictionary = jObject?.ToObject<Dictionary<string, object>>();

            var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
            string result;

            try
            {
                result = await actionMethod(graphClient, jObject);
                await SendConfirmedCard(turnContext, actionName, parametersDictionary?.ExcludeVerb(), cancellationToken);
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            turnState.Temp.Input = turnContext.GetActionSubmitText(actionName, result);
        }

        protected async Task<string> SendGraphConfirmationCard(
            ITurnContext turnContext,
            string actionName,
            Dictionary<string, object> parameters,
            Func<GraphServiceClient, Task<IEnumerable<(ParameterAttribute, string)>>>? fetchGraphData = null)
        {
            var paramAttributes = GetActionParameters(actionName)?.ToList() ?? [];

            if (fetchGraphData != null)
            {
                var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
                var graphData = await fetchGraphData(graphClient);

                foreach (var (paramAttribute, value) in graphData)
                {
                    parameters[paramAttribute.Name] = value;
                }

                paramAttributes.AddRange(graphData.Select(g => g.Item1));
            }

            return await SendConfirmationCard(turnContext, actionName, parameters, paramAttributes);
        }
    }
}
