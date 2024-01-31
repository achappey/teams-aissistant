using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateInvoicePlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Invoices")
    {

        [Action("Simplicate.SearchInvoices")]
        [Description("Search for invoices in Simplicate")]
        [Parameter(name: "invoice_number", type: "string", description: "Invoice number")]
        [Parameter(name: "project.name", type: "string", description: "Name of the project")]
        [Parameter(name: "project.project_manager.name", type: "string", description: "Name of the project manager")]
        [Parameter(name: "limit", type: "number", maximum: 100, description: "Item limit of the query")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "organization.name", type: "string", description: "Name of the organization")]
        [Parameter(name: "my_organization_profile.organization.name", type: "string", description: "Name of the my organization")]
        [Parameter(name: "date][ge", type: "string", format: "date-time",
            description: "Invoice date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "date][le", type: "string", format: "date-time",
            description: "Invoice date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchInvoices([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName,
             [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/invoice", "-date");
        }

        [Action("Simplicate.SearchPropositions")]
        [Description("Search for propositions in Simplicate")]
        [Parameter(name: "project.name", type: "string", description: "Name of the project")]
        [Parameter(name: "project.project_number", type: "string", description: "Number of the project")]
        [Parameter(name: "project.project_manager.name", type: "string", description: "Name of the project manager")]
        [Parameter(name: "project.organization.name", type: "string", description: "Name of the organization")]
        public Task<string> SearchPropositions([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/proposition", "-created_at");
        }

        [Action("Simplicate.SearchInvoiceDocuments")]
        [Description("Search for invoice documents in Simplicate")]
        [Parameter(name: "title", type: "string", description: "Title of the invoice document")]
        [Parameter(name: "description", type: "string", description: "Description of the invoice document")]
        [Parameter(name: "document_type.label", type: "string", description: "Label of the document type")]
        [Parameter(name: "created_by.name", type: "string", description: "Name of the created by")]
        public Task<string> SearchInvoiceDocuments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/document", "-created_at");
        }

        [Action("Simplicate.SearchDebtors")]
        [Description("Search for debtors in Simplicate")]
        [Parameter(name: "organization.name", type: "string", description: "Name of the organization")]
        [Parameter(name: "organization.relation_number", type: "string", description: "Relation number of the organization")]
        [Parameter(name: "organization.relation_type.label", type: "string", description: "Label of the organization relation type")]
        [Parameter(name: "person.full_name", type: "string", description: "Full name of the person")]
        public Task<string> SearchDebtors([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/debtor");
        }

        [Action("Simplicate.SearchPayments")]
        [Description("Search for payments in Simplicate")]
        [Parameter(name: "description", type: "string", description: "Description of the payment")]
        [Parameter(name: "date][ge", type: "string", format: "date-time",
            description: "Payment date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "date][le", type: "string", format: "date-time",
            description: "Payment date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchPayments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "invoices/payment", "-date");
        }

        [Action("Simplicate.SearchReverseInvoices")]
        [Description("Search for reverse invoices in Simplicate")]
        [Parameter(name: "invoice_number", type: "string", description: "Invoice number")]
        [Parameter(name: "project.name", type: "string", description: "Name of the project")]
        [Parameter(name: "date][ge", type: "string", format: "date-time",
          description: "Payment date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "date][le", type: "string", format: "date-time",
          description: "Payment date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchReverseInvoices([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/reverseinvoice", "-date");
        }
    }
}
