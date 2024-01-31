using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSecurityPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Security")
    {

        [Action("MicrosoftGraph.ListSecureScores")]
        [Description("Lists secure scores with Microsoft Graph")]
        public Task<string> ListSecureScores([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.SecureScores
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListSecurityAlerts")]
        [Description("Lists security alerts with Microsoft Graph")]
        public Task<string> ListSecurityAlerts([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.Alerts_v2
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListThreatAssessments")]
        [Description("Lists threat assessments with Microsoft Graph")]
        public Task<string> ListThreatAssessments([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.InformationProtection.ThreatAssessmentRequests
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListSecurityIncidents")]
        [Description("Lists security incidents with Microsoft Graph")]
        public Task<string> ListSecurityIncidents([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Security.Incidents
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.RunHuntingQuery")]
        [Description("Queries a specified set of event, activity, or entity data supported by Microsoft 365 Defender to proactively look for specific threats in your environment")]
        [Parameter(name: "query", type: "string", required: true, description: "The hunting query in Kusto Query Language (KQL)")]
        public Task<string> RunHuntingQuery([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Security.MicrosoftGraphSecurityRunHuntingQuery.PostAsync(new()
                {
                    Query = parameters["query"]?.ToString(),
                }));
        }
    }
}
