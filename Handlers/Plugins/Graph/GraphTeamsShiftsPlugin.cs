using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphTeamsShiftsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Teams Shifts")
    {
        [Action("MicrosoftGraph.ListTeamShifts")]
        [Description("Get the list of shift instances in a schedule")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamShifts([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Schedule.Shifts
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListTeamOpenShifts")]
        [Description("Get the list of open shift instances in a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamOpenShifts([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Schedule.OpenShifts
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListTeamsDayNotes")]
        [Description("Get the list of day notes in a team")]
        [Parameter(name: "teamsId", type: "string", required: true, description: "Id of the team")]
        public Task<string> ListTeamsDayNotes([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
               [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teams[parameters["teamsId"]?.ToString()].Schedule.DayNotes
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListWorkforceIntegrations")]
        [Description("Get the list of workforce integrations")]
        public Task<string> ListWorkforceIntegrations([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Teamwork.WorkforceIntegrations
                            .GetAsync();

                        return result?.Value;
                    });
        }

    }
}
