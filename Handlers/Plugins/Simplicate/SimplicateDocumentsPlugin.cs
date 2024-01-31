using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateDocumentsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Documents")
    {

        [Action("Simplicate.SearchDocuments")]
        [Description("Search for documents in Simplicate")]
        [Parameter(name: "title", type: "string", description: "Title of the document")]
        [Parameter(name: "description", type: "string", description: "Description of the document")]
        [Parameter(name: "document_type.label", type: "string", description: "Label of the document type")]
        [Parameter(name: "created_by.name", type: "string", description: "Name of created by")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchDocuments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "documents/document");
        }

        [Action("Simplicate.SearchDocumentTypes")]
        [Description("Search for document types in Simplicate")]
        [Parameter(name: "label", type: "string", description: "Label of the document type")]
        [Parameter(name: "description", type: "string", description: "Description of the document type")]
        public Task<string> SearchDocumentTypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "documents/documenttype");
        }
    }
}
