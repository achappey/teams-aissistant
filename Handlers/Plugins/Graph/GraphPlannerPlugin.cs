using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.Extensions;
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
    public class GraphPlannerPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Planner")
    {
        [Action("MicrosoftGraph.ListPlanners")]
        [Description("Lists planners with Microsoft Graph")]
        public Task<string> ListPlanners([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Planner
                            .GetAsync();

                        return result?.Plans;
                    });
        }

        [Action("MicrosoftGraph.ListAllPlannerTasks")]
        [Description("Lists all planner tasks with Microsoft Graph")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> ListAllPlannerTasks([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Planner.Tasks
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                                    requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListPlannerBuckets")]
        [Description("Lists planner buckets by planner id with Microsoft Graph")]
        [Parameter(name: "plannerId", type: "string", required: true, description: "Id of the planner")]
        public Task<string> ListPlannerBuckets([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Planner.Plans[parameters["plannerId"]?.ToString()].Buckets
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListPlannerTasks")]
        [Description("Lists planner tasks by planner id with Microsoft Graph")]
        [Parameter(name: "plannerId", type: "string", required: true, description: "Id of the planner")]
        public Task<string> ListPlannerTasks([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Planner.Plans[parameters["plannerId"]?.ToString()].Tasks
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreatePlannerTask")]
        [Description("Creates a new planner task")]
        [Parameter(name: "planId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the planner")]
        [Parameter(name: "bucketId", type: "string", required: true, readOnly: true, visible: false, description: "Id of the bucket")]
        [Parameter(name: "title", type: "string", required: true, description: "Title of the task")]
        [Parameter(name: "description", type: "string", required: true, description: "Description of the task")]
        [Parameter(name: "assignTo", type: "string", readOnly: true, description: "Id of the user to assign the task")]
        public Task<string> CreatePlannerTask([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
             async (GraphServiceClient graphClient) =>
             {
                 var planId = parameters["planId"]?.ToString();
                 var bucketId = parameters["bucketId"]?.ToString();
                 var assignToId = parameters["assignTo"]?.ToString();

                 var bucket = await graphClient.Planner.Plans[parameters["planId"]?.ToString()].Buckets[parameters["bucketId"]?.ToString()].GetAsync();
                 var planner = await graphClient.Planner.Plans[parameters["planId"]?.ToString()].GetAsync();
                 var assignTo = await graphClient.Users[parameters["assignTo"]?.ToString()].GetAsync();

                 var plannerName = planner?.Title ?? string.Empty;
                 var bucketName = bucket?.Name ?? string.Empty;
                 var assignToName = assignTo?.DisplayName ?? string.Empty;

                 return [
                     (new ParameterAttribute(name: "Planner", type: "string", readOnly: true), plannerName),
                        (new ParameterAttribute(name: "Bucket", type: "string", readOnly: true), bucketName),
                        (new ParameterAttribute(name: "Assign to", type: "string", readOnly: true), assignToName)
                 ];
             });
        }

        [Submit]
        public Task MicrosoftGraphCreatePlannerTaskSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreatePlannerTask", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
            {
                var requestBody = new PlannerTask
                {
                    PlanId = jObject?["planId"]?.ToString(),
                    BucketId = jObject?["bucketId"]?.ToString(),
                    Title = jObject?["title"]?.ToString(),
                    Details = new()
                    {
                        Description = jObject?["description"]?.ToString(),

                    },
                    Assignments = new()
                    {
                        AdditionalData = new Dictionary<string, object>(),
                    },
                };

                if (jObject?["assignTo"] != null)
                {
                    requestBody.Assignments.AdditionalData.Add(jObject["assignTo"]?.ToString(),
                        new PlannerAssignment { OdataType = "#microsoft.graph.plannerAssignment", OrderHint = " !" });
                }

                var result = await graphClient.Planner.Tasks.PostAsync(requestBody);

                return JsonConvert.SerializeObject(result);
            }, cancellationToken);
        }

    }
}
