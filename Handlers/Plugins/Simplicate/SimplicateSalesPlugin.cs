using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateSalesPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Sales")
    {

        [Action("Simplicate.SearchSales")]
        [Description("Search for sales in Simplicate")]
        [Parameter(name: "subject", type: "string", description: "Subject of the sales")]
        [Parameter(name: "responsible_employee.name", type: "string", description: "Responsible employee of the sales")]
        [Parameter(name: "teams.name", type: "string", description: "Name of the team")]
        [Parameter(name: "organization.name", type: "string", description: "Name of the organization")]
        [Parameter(name: "person.full_name", type: "string", description: "Full name of the person")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchSales([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/sales");
        }

        [Action("Simplicate.SearchQuotes")]
        [Description("Search for quotes in Simplicate")]
        [Parameter(name: "quote_subject", type: "string", description: "Subject of the quote")]
        [Parameter(name: "sales_id", type: "string", description: "Id of the sales")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "sent_at][ge", type: "string", format: "date-time",
            description: "Sent at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "sent_at][le", type: "string", format: "date-time",
            description: "Sent at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchQuotes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "sales/quote");
        }

        [Action("Simplicate.AddNewQuote")]
        [Description("Adds a new quote in Simplicate")]
        [Parameter(name: "quotetemplate_id", type: "string", required: true, description: "Id of the quote template")]
        [Parameter(name: "sales_id", type: "string", required: true, description: "Id of the sales")]
        [Parameter(name: "quote_subject", type: "string", required: true, description: "Subject of the quote")]
        public Task<string> AddNewQuote([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task SimplicateAddNewQuoteSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitNewActionAsync(turnContext, turnState, "Simplicate.AddNewQuote", data, "sales/quote", cancellationToken);
        }
/*
        [Action("AddNewSales")]
        [Description("Adds a new sales in Simplicate")]
        [Parameter(name: "subject", type: "string", required: true, description: "Subject of the sales")]
        [Parameter(name: "start_date", type: "string", format: "date-time",
            description: "Start date of the sales (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "expected_closing_date", type: "string", format: "date-time",
            description: "Expected closing date of the sales (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "expected_revenue", type: "number", description: "Expected revenue of the sales")]
        [Parameter(name: "chance_to_score", type: "number", minimum: 0, maximum: 100, description: "Chance to score")]
        [Parameter(name: "note", type: "string", multiline: true, description: "Note for the sales")]
        public Task<string> AddNewSales([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task AddNewSalesSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitNewActionAsync(turnContext, turnState, "AddNewSales", data, "sales/sales", cancellationToken);
        }*/
    }
}
