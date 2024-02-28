using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateSalesSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
     GraphClientServiceProvider graphClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
        : SimplicateBasePlugin(simplicateClientServiceProvider, graphClientServiceProvider, proactiveMessageService, driveRepository, "Sales Settings")
    {

        [Action("Simplicate.GetQuoteTemplates")]
        [Description("Gets quote templates in Simplicate")]
        public Task<string> GetQuoteTemplates([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/quotetemplate");
        }

        [Action("Simplicate.GetSalesSources")]
        [Description("Gets sales sources in Simplicate")]
        public Task<string> GetSalesSources([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/salessource");
        }

        [Action("Simplicate.GetSalesReasons")]
        [Description("Gets sales reasons in Simplicate")]
        public Task<string> GetSalesReasons([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/salesreason");
        }

        [Action("Simplicate.GetSalesProgress")]
        [Description("Gets sales progress in Simplicate")]
        public Task<string> GetSalesProgress([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/salesprogress");
        }

        [Action("Simplicate.GetSalesDocumentTypes")]
        [Description("Gets sales document types in Simplicate")]
        public Task<string> GetSalesDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/documenttype");
        }

        [Action("Simplicate.GetSalesRevenueGroups")]
        [Description("Gets sales document types in Simplicate")]
        public Task<string> GetSalesRevenueGroups([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/revenuegroup");
        }


    }
}
