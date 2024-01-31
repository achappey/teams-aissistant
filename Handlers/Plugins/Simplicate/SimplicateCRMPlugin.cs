using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateCRMPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) 
            : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "CRM")
    {

        [Action("Simplicate.SearchContactPersons")]
        [Description("Search for contactpersons in Simplicate")]
        [Parameter(name: "person.full_name", type: "string", description: "Full name of the person")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "organization.name", type: "string", description: "Name of the organization")]
        [Parameter(name: "work_function", type: "string", description: "Job title of the contactperson")]
        [Parameter(name: "work_email", type: "string", description: "Email of the contactperson")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
                   description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
                   description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchContactPersons([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
                    [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/contactperson");
        }

        [Action("Simplicate.SearchOrganizations")]
        [Description("Search for organizations in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the organization")]
        [Parameter(name: "relation_manager.name", type: "string", description: "Relation manager of the organization")]
        [Parameter(name: "visiting_address.locality", type: "string", description: "City of the organization")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "teams.name", type: "string", description: "Name of the team")]
        [Parameter(name: "industry.name", type: "string", description: "Name of the industry")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchOrganizations([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/organization");
        }

        [Action("Simplicate.SearchPersons")]
        [Description("Search for persons in Simplicate")]
        [Parameter(name: "full_name", type: "string", description: "Full name of the person")]
        [Parameter(name: "relation_manager.name", type: "string", description: "Relation manager of the person")]
        [Parameter(name: "teams.name", type: "string", description: "Name of the team")]
        [Parameter(name: "offset", type: "number", description: "Offset of the query")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchPersons([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "crm/person");
        }

        [Action("Simplicate.AddNewOrganization")]
        [Description("Adds a new organization in Simplicate CRM")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the organization")]
        [Parameter(name: "email", type: "string", format:"email", description: "Email of the organization")]
        [Parameter(name: "linkedin_url", type: "string", format:"uri", description: "LinkedIn url of the organization")]
        [Parameter(name: "url", type: "string", format:"uri", description: "Website of the organization")]
        [Parameter(name: "coc_code", type: "string", description: "Coc code")]
        [Parameter(name: "vat_number", type: "string", description: "VAT number")]
        [Parameter(name: "phone", type: "string", format:"tel", description: "Phone of the organization")]
        [Parameter(name: "note", type: "string", multiline: true, description: "Note for the organization")]
        [Parameter(name: "relation_type.id", type: "string", readOnly: true, description: "Id of the relation type")]
        [Parameter(name: "industry.id", type: "string", readOnly: true, description: "Id of the industry")]
        [Parameter(name: "visiting_address.locality", type: "string", description: "Locality of the visiting address")]
        public async Task<string> AddNewOrganization([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var properties = GetActionParameters(actionName).ToList();
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "industry.id", "crm/industry", "Industry", "name");
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "relation_type.id", "crm/relationype", "Relationtype", "label");

            return await SendConfirmationCard(turnContext, actionName, parameters, properties);
        }

        [Submit]
        public Task SimplicateAddNewOrganizationSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitNewActionAsync(turnContext, turnState, "Simplicate.AddNewOrganization", data, "crm/organization", cancellationToken);
        }

        [Action("Simplicate.UpdateOrganization")]
        [Description("Updates an organization in Simplicate CRM")]
        [Parameter(name: "id", type: "string", required: true, readOnly: true, description: "Id of the organization")]
        [Parameter(name: "name", type: "string", description: "Name")]
        [Parameter(name: "email", type: "string", format:"email", description: "Email")]
        [Parameter(name: "linkedin_url", type: "string", format:"uri", description: "LinkedIn url")]
        [Parameter(name: "url", type: "string", description: "Website")]
        [Parameter(name: "phone", type: "string", format:"tel", description: "Phone number")]
        [Parameter(name: "coc_code", type: "string", description: "Coc code")]
        [Parameter(name: "vat_number", type: "string", description: "VAT number")]
        [Parameter(name: "industry.id", type: "string", readOnly: true, description: "Id of the industry")]
        [Parameter(name: "relation_type.id", type: "string", readOnly: true, description: "Id of the relation type")]
        [Parameter(name: "note", type: "string", multiline: true, description: "Note for the organization")]
        [Parameter(name: "visiting_address.locality", type: "string", description: "Locality of the visiting address")]
        public async Task<string> UpdateOrganization([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var properties = GetActionParameters(actionName).ToList();
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "industry.id", "crm/industry", "Industry", "name");
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "relation_type.id", "crm/relationype", "Relationtype", "label");

            return await SendConfirmationCard(turnContext, actionName, parameters, properties);
        }

        [Submit]
        public Task SimplicateUpdateOrganizationSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitUpdateActionAsync(turnContext, turnState, "Simplicate.UpdateOrganization", data, "crm/organization", cancellationToken);
        }

        [Action("Simplicate.AddNewPerson")]
        [Description("Adds a new person in Simplicate CRM")]
        [Parameter(name: "family_name", type: "string", required: true, description: "Family name")]
        [Parameter(name: "initials", type: "string", description: "Initials")]
        [Parameter(name: "full_name", type: "string", description: "Full name")]
        [Parameter(name: "first_name", type: "string", description: "First name")]
        [Parameter(name: "date_of_birth", type: "string", format:"date-time", description: "Date of birth")]
        [Parameter(name: "linkedin_url", type: "string", format:"uri", description: "LinkedIn url")]
        [Parameter(name: "phone", type: "string", format:"tel", description: "Mobile phone")]
        [Parameter(name: "relation_type.id", type: "string", readOnly: true, description: "Id of the relation type")]
        [Parameter(name: "gender_id", type: "string", readOnly: true, description: "Id of the gender")]
        [Parameter(name: "email", type: "string", format:"email", description: "Email")]
        [Parameter(name: "note", type: "string", multiline: true, description: "Notes")]
        public async Task<string> AddNewPerson([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var properties = GetActionParameters(actionName).ToList();
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "relation_type.id", "crm/relationtype", "Relation type", "label");
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "gender_id", "crm/gender", "Gender", "name");

            return await SendConfirmationCard(turnContext, actionName, parameters, properties);
        }

        [Submit]
        public Task SimplicateAddNewPersonSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitNewActionAsync(turnContext, turnState, "Simplicate.AddNewPerson", data, "crm/person", cancellationToken);
        }

        [Action("Simplicate.UpdatePerson")]
        [Description("Updates a person in Simplicate CRM")]
        [Parameter(name: "id", type: "string", required: true, readOnly: true, description: "Id of the person")]
        [Parameter(name: "family_name", type: "string", description: "Family name")]
        [Parameter(name: "initials", type: "string", description: "Initials")]
        [Parameter(name: "full_name", type: "string", description: "Full name")]
        [Parameter(name: "first_name", type: "string", description: "First name")]
        [Parameter(name: "date_of_birth", type: "string", format:"date-time", description: "Date of birth")]
        [Parameter(name: "is_active", type: "boolean", description: "Is active")]
        [Parameter(name: "phone", type: "string", description: "Mobile phone")]
        [Parameter(name: "relation_type.id", type: "string", readOnly: true, description: "Id of the relation type")]
        [Parameter(name: "gender_id", type: "string", readOnly: true, description: "Id of the gender")]
        [Parameter(name: "email", type: "string", description: "Email")]
        [Parameter(name: "note", type: "string", multiline: true, description: "Notes")]
        public async Task<string> UpdatePerson([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var properties = GetActionParameters(actionName).ToList();
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "relation_type.id", "crm/relationtype", "Relation type", "label");
            await AddParameterPropertiesAsync(turnContext, parameters, properties, "gender_id", "crm/gender", "Gender", "name");

            return await SendConfirmationCard(turnContext, actionName, parameters, properties);
        }

        [Submit]
        public Task SimplicateUpdatePersonSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitUpdateActionAsync(turnContext, turnState, "Simplicate.UpdatePerson", data, "crm/person", cancellationToken);
        }
    }
}
