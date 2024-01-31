using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphPlannerBucketsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Planner Buckets")
    {
        [Action("MicrosoftGraph.CreatePlannerBucket")]
        [Description("Creates a new planner bucket")]
        [Parameter(name: "planId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the planner")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the bucket")]
        public Task<string> CreatePlannerBucket([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var planId = parameters["planId"]?.ToString();
                    var planner = await graphClient.Planner.Plans[planId].GetAsync();

                    var plannerTitle = planner?.Title ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Planner", type: "string", readOnly: true), plannerTitle)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphCreatePlannerBucketSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreatePlannerBucket", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new PlannerBucket
                    {
                        Name = jObject?["name"]?.ToString(),
                        PlanId = jObject?["planId"]?.ToString(),
                        OrderHint = " !",
                    };

                    var result = await graphClient.Planner.Buckets.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdatePlannerBucket")]
        [Description("Updates a planner bucket")]
        [Parameter(name: "planId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the planner")]
        [Parameter(name: "bucketId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the bucket")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the bucket")]
        public Task<string> UpdatePlannerBucket([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var planId = parameters["planId"]?.ToString();
                    var planner = await graphClient.Planner.Plans[planId].GetAsync();
                    var plannerTitle = planner?.Title ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Planner", type: "string", readOnly: true), plannerTitle)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphUpdatePlannerBucketSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdatePlannerBucket", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new PlannerBucket
                {
                    Name = jObject?["name"]?.ToString(),
                };

                var result = await graphClient.Planner.Buckets[jObject?["bucketId"]?.ToString()].PatchAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeletePlannerBucket")]
        [Description("Deletes a planner bucket")]
        [Parameter(name: "planId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the planner")]
        [Parameter(name: "bucketId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the bucket")]
        public Task<string> DeletePlannerBucket([ActionTurnContext] TurnContext turnContext,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var planId = parameters["planId"]?.ToString();
                    var bucketId = parameters["bucketId"]?.ToString();

                    var planner = await graphClient.Planner.Plans[planId].GetAsync();
                    var bucket = await graphClient.Planner.Buckets[bucketId].GetAsync();

                    var plannerTitle = planner?.Title ?? string.Empty;
                    var bucketName = bucket?.Name ?? string.Empty;

                    return new List<(ParameterAttribute, string)>
                    {
                        (new ParameterAttribute(name: "Planner", type: "string", readOnly: true), plannerTitle),
                        (new ParameterAttribute(name: "Bucket", type: "string", readOnly: true), bucketName)
                    };
                });
        } 

        [Submit]
        public Task MicrosoftGraphDeletePlannerBucketSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeletePlannerBucket", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                await graphClient.Planner.Buckets[jObject?["bucketId"]?.ToString()].DeleteAsync();

                return "Bucket deleted";
            }, cancellationToken);
        }
    }
}
