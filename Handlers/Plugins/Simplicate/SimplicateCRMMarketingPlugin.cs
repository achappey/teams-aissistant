using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateCRMMarketingPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
                : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "CRM Marketing")
    {
        [Action("Simplicate.SearchOrganizationContactPersons")]
        [Description("Search for contactpersons by organizations in Simplicate")]
        [Parameter(name: "relation_manager.name", type: "string", description: "Relation manager of the organization")]
        [Parameter(name: "visiting_address.locality", type: "string", description: "City of the organization")]
        [Parameter(name: "teams.name", type: "string", description: "Name of the team")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "industry.name", type: "string", description: "Name of the industry")]
        public Task<string> SearchOrganizationContactPersons([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/organization", null, "linked_persons_contacts");
        }

    }
}
