using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class LuchtmeetnetSettingsPlugin(IHttpClientFactory clientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
            : LuchtmeetnetBasePlugin(clientFactory, proactiveMessageService, driveRepository, "Settings")
    {

        [Action("Luchtmeetnet.GetComponent")]
        [Description("Gets luchtmeetnet component details by formula")]
        [Parameter(name: "formula", type: "string", required: true, description: "Formula of the component")]
        public Task<string> GetComponent([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
               [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, $"components/{parameters["formula"]}");
        }

        [Action("Luchtmeetnet.GetComponents")]
        [Description("Gets luchtmeetnet components")]
        [Parameter(name: "page", type: "number", description: "Page number")]
        [Parameter(name: "order_by", type: "string", enumValues: [OrderComponentsByConstants.Order,
            OrderComponentsByConstants.Formula, OrderComponentsByConstants.NameNl, OrderComponentsByConstants.NameEn],
            description: "Order by")]
        public Task<string> GetComponents([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "components");
        }

        [Action("Luchtmeetnet.GetStation")]
        [Description("Gets luchtmeetnet station details by number")]
        [Parameter(name: "number", type: "string", required: true, description: "Number of the station")]
        public Task<string> GetStation([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, $"stations/{parameters["number"]}");
        }

        [Action("Luchtmeetnet.GetStations")]
        [Description("Gets luchtmeetnet stations")]
        [Parameter(name: "page", type: "number", description: "Page number")]
        [Parameter(name: "order_by", type: "string", enumValues: [OrderStationsByConstants.Number, OrderStationsByConstants.Location],
           description: "Order by")]
        public Task<string> GetStations([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "stations");
        }

        [Action("Luchtmeetnet.GetOrganisations")]
        [Description("Gets luchtmeetnet organisations")]
        [Parameter(name: "page", type: "number", description: "Page number")]
        public Task<string> GetOrganisations([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "organisations");
        }
    }
}
