using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL
{
    public class NLGovernmentNewsPlugin(IHttpClientFactory clientFactory,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : NLGovernmentBasePlugin(clientFactory, proactiveMessageService, driveRepository, "News")
    {
        private const string BASEURL = "infotypes/news";

        [Action("Rijksoverheid.GetNewsDetail")]
        [Description("Gets detailed information from NL government news")]
        [Parameter(name: "id", type: "string", required: true, description: "Id of the news")]
        public Task<string> GetNewsDetail([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
         [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentItem(turnContext, turnState, actionName, parameters, BASEURL);
        }

        [Action("Rijksoverheid.SearchNews")]
        [Description("Search for NL government news")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchNews([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters, $"{BASEURL}");
        }

        [Action("Rijksoverheid.SearchNewsBySubject")]
        [Description("Search for NL government news by subject")]
        [Parameter(name: "subject", type: "string", required: true, description: "Name of the subject")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchNewsBySubject([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters,
                $"{BASEURL}/subjects/{parameters["subject"]}", ["subject"]);
        }

        [Action("Rijksoverheid.SearchNewsByMinistry")]
        [Description("Search for NL government news by subject")]
        [Parameter(name: "organisationalunit", type: "string", required: true, description: "Name of the NL government ministry")]
        [Parameter(name: "lastmodifiedsince", type: "string", description: "Last modified since (format: YYYYMMDD, ex 20171101)")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "rows", type: "number", maximum: 200, description: "Number of items")]
        public Task<string> SearchNewsByMinistry([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return GetNLGovernmentList(turnContext, turnState, actionName, parameters,
                $"{BASEURL}/ministries/{parameters["organisationalunit"]}", ["organisationalunit"]);
        }
    }
}
