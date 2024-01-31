using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateInvoiceSettingsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Invoices Settings")
    {

        [Action("Simplicate.SearchInvoicePaymentTerms")]
        [Description("Search for invoice payment terms in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the payment term")]
        public Task<string> SearchInvoicePaymentTerms([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/paymentterm");
        }

        [Action("Simplicate.SearchInvoiceStatus")]
        [Description("Search for invoice status in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the invoice status")]
        public Task<string> SearchInvoiceStatus([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/invoicestatus");
        }

        [Action("Simplicate.SearchInvoiceReminderSets")]
        [Description("Search for invoice reminder sets in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the reminder set")]
        public Task<string> SearchInvoiceReminderSets([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                 [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/reminderset");
        }

        [Action("Simplicate.SearchInvoiceReminderTemplates")]
        [Description("Search for invoice reminder templates in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the reminder template")]
        [Parameter(name: "subject", type: "string", description: "Subject of the reminder template")]
        public Task<string> SearchInvoiceReminderTemplates([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/remindertemplate");
        }

        [Action("Simplicate.SearchInvoiceVatClasses")]
        [Description("Search for invoice VAT classes in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the VAT class")]
        [Parameter(name: "code", type: "string", description: "Code of the VAT class")]
        public Task<string> SearchInvoiceVatClasses([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/vatclass");
        }

        [Action("Simplicate.GetInvoiceDocumentTypes")]
        [Description("Gets invoice document types in Simplicate")]
        public Task<string> GetInvoiceDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/documenttype");
        }


    }
}
