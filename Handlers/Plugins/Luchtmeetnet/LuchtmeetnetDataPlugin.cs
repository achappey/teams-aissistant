using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class LuchtmeetnetDataPlugin(IHttpClientFactory clientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
            : LuchtmeetnetBasePlugin(clientFactory, proactiveMessageService, driveRepository, "Data")
    {
        [Action("Luchtmeetnet.GetMeasurements")]
        [Description("Gets luchtmeetnet measurements")]
        [Parameter(name: "station_number", type: "string", description: "Number of the station")]
        [Parameter(name: "formula", type: "string", description: "Name of the formula")]
        [Parameter(name: "start", type: "string", required: true, description: "IS08601 representation of start of measurements (eg. 2018-12-01T09:00:00Z)")]
        [Parameter(name: "end", type: "string", required: true, description: "IS08601 representation of end of measurements")]
        [Parameter(name: "page", type: "number", description: "Page number")]
        [Parameter(name: "order_by", type: "string", enumValues: [OrderMeasurementsByConstants.Formula,
            OrderMeasurementsByConstants.TimestampMeasured], description: "Order by")]
        [Parameter(name: "order_direction", type: "string", enumValues: [OrderByDirectionConstants.Asc,
            OrderByDirectionConstants.Desc], description: "Order direction")]
        public Task<string> GetMeasurements([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "measurements");
        }

        [Action("Luchtmeetnet.GetLKI")]
        [Description("Gets luchtmeetnet LKI")]
        [Parameter(name: "station_number", type: "string", description: "Number of the station")]
        [Parameter(name: "start", type: "string", required: true, description: "IS08601 representation of start of measurements (eg. 2018-12-01T09:00:00Z)")]
        [Parameter(name: "end", type: "string", required: true, description: "IS08601 representation of end of measurements")]
        [Parameter(name: "page", type: "number", description: "Page number")]
        [Parameter(name: "order_by", type: "string", enumValues: [OrderMeasurementsByConstants.TimestampMeasured], description: "Order by")]
        [Parameter(name: "order_direction", type: "string", enumValues: [OrderByDirectionConstants.Asc,
            OrderByDirectionConstants.Desc], description: "Order direction")]
        public Task<string> GetLKI([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "lki");
        }

        [Action("Luchtmeetnet.GetConcentrations")]
        [Description("Gets luchtmeetnet concentrations")]
        [Parameter(name: "station_number", type: "string", description: "Number of the station")]
        [Parameter(name: "longitude", type: "string", required: true, description: "Longitude of the location")]
        [Parameter(name: "latitude", type: "string", required: true, description: "Latitude of the location")]
        [Parameter(name: "formula", type: "string", description: "Formula",
            enumValues: [FormulaConstants.LKI, FormulaConstants.O3, FormulaConstants.NO2, FormulaConstants.PM10])]
        [Parameter(name: "start", type: "string", required: true, description: "IS08601 representation of start of measurements (eg. 2018-12-01T09:00:00Z)")]
        [Parameter(name: "end", type: "string", required: true, description: "IS08601 representation of end of measurements")]
        public Task<string> GetConcentrations([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetList(turnContext, turnState, actionName, parameters, "concentrations");
        }
    }
}
