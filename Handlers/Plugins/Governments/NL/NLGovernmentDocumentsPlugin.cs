using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Teams.AI;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class NLGovernmentDocumentsPlugin(TeamsAdapter teamsAdapter,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : NLGovernmentBasePlugin(teamsAdapter, proactiveMessageService, driveRepository, "Documents")
    {
        [Action("Rijksoverheid.GetDocumentDetail")]
        [Description("Gets detailed information from an NL government document")]
        [Parameter(name: "id", type: "string", required: true, description: "Id of the document")]
        public Task<string> GetDocumentDetail([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentItem(turnContext, turnState, actionName, parameters, "documents");
        }

        [Action("Rijksoverheid.SearchDocuments")]
        [Description("Search for NL government documents")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "subject", type: "string", description: "Subject of the document")]
        [Parameter(name: "type", type: "string", description: "Name of the NL government infotype")]
        [Parameter(name: "organisationalunit", type: "string", description: "Name of the NL government ministry")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchDocuments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, "documents");
        }

        [Action("Rijksoverheid.GetInfotypes")]
        [Description("Gets a list of NL government infotypes")]
        public Task<string> GetInfotypes([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, "infotypes/infotypes");
        }
    }
}
