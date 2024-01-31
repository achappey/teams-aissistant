using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class NLGovernmentFaqsPlugin(IHttpClientFactory clientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : NLGovernmentBasePlugin(clientFactory, proactiveMessageService, driveRepository, "FAQs")
    {
        private const string BASEURL = "infotypes/faq";

        [Action("Rijksoverheid.GetFAQDetail")]
        [Description("Gets detailed information from NL government FAQ")]
        [Parameter(name: "id", type: "string", required: true, description: "Id of the FAQ")]
        public Task<string> GetFAQDetail([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentItem(turnContext, turnState, actionName, parameters, BASEURL);
        }

        [Action("Rijksoverheid.SearchFAQs")]
        [Description("Search for NL government FAQs")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchFAQs([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, $"{BASEURL}");
        }

        [Action("Rijksoverheid.SearchFAQsBySubject")]
        [Description("Search for NL government FAQs by subject")]
        [Parameter(name: "subject", type: "string", required: true, description: "Name of the subject")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchFAQsBySubject([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters,
                $"{BASEURL}/subjects/{parameters["subject"]}", ["subject"]);
        }

        [Action("Rijksoverheid.SearchFAQsByMinistry")]
        [Description("Search for NL government FAQs by subject")]
        [Parameter(name: "organisationalunit", type: "string", required: true, description: "Name of the NL government ministry")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchFAQsByMinistry([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters,
                $"{BASEURL}/ministries/{parameters["organisationalunit"]}", ["organisationalunit"]);
        }
    }
}
