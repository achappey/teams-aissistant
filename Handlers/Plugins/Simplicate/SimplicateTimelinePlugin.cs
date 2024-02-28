using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateTimelinePlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
    GraphClientServiceProvider graphClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
        : SimplicateBasePlugin(simplicateClientServiceProvider, graphClientServiceProvider, proactiveMessageService, driveRepository, "Timeline")
    {

        [Action("Simplicate.SearchTimeline")]
        [Description("Search the timeline in Simplicate")]
        [Parameter(name: "title", type: "string", description: "Title of the timeline message")]
        [Parameter(name: "content", type: "string", description: "Content of the timeline message")]
        [Parameter(name: "created_by.label", type: "string", description: "Name of the created by")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchTimeline([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "timeline/message", "-created_at");
        }

        [Action("Simplicate.SearchTimelineMessageTypes")]
        [Description("Search the timeline message types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the timeline message type")]
        public Task<string> SearchTimelineMessageTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "timeline/messagetype");
        }
    }
}
