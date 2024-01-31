using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate
{
    public class SimplicateProjectsPlugin(SimplicateClientServiceProvider simplicateClientServiceProvider,
        ProactiveMessageService proactiveMessageService, DriveRepository driveRepository) : SimplicateBasePlugin(simplicateClientServiceProvider, proactiveMessageService, driveRepository, "Projects")
    {

        [Action("Simplicate.SearchProjects")]
        [Description("Search for projects in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the project")]
        [Parameter(name: "project_manager.name", type: "string", description: "Name of the project manager")]
        [Parameter(name: "organization.name", type: "string", description: "Name of the organization")]
        [Parameter(name: "employees.name", type: "string", description: "Name of the employee")]
        [Parameter(name: "my_organization_profile.organization.name", type: "string", description: "Name of my organization")]
        [Parameter(name: "created_at][ge", type: "string", format: "date-time",
            description: "Created at greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "created_at][le", type: "string", format: "date-time",
            description: "Created at less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchProjects([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/project");
        }

        [Action("Simplicate.SearchProjectDocuments")]
        [Description("Search for project documents in Simplicate")]
        [Parameter(name: "title", type: "string", description: "Title of the project document")]
        [Parameter(name: "description", type: "string", description: "Description of the project document")]
        [Parameter(name: "document_type.label", type: "string", description: "Label of the document type")]
        [Parameter(name: "created_by.name", type: "string", description: "Name of the created by")]
        public Task<string> SearchProjectDocuments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
             [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/document", "-created_at");
        }

        [Action("Simplicate.SearchProjectServices")]
        [Description("Search for project services in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the project service")]
        [Parameter(name: "service_number", type: "string", description: "Project service number")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
         description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
         description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "invoice_date][ge", type: "string", format: "date-time",
         description: "Invoice date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "invoice_date][le", type: "string", format: "date-time",
         description: "Invoice date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchProjectServices([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/service");
        }

        [Action("Simplicate.SearchProjectAssignments")]
        [Description("Search for project assignment in Simplicate")]
        [Parameter(name: "name", type: "string", description: "Name of the project assignment")]
        [Parameter(name: "description", type: "string", description: "Description of the project assignment")]
        [Parameter(name: "service_number", type: "string", description: "Project service number")]
        [Parameter(name: "start_date][ge", type: "string", format: "date-time",
            description: "Start date greater than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        [Parameter(name: "start_date][le", type: "string", format: "date-time",
            description: "Start date less than or equals (format: yyyy-MM-dd HH:mm:ss)")]
        public Task<string> SearchProjectAssignments([ActionTurnContext] ITurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SearchItems(turnContext, turnState, actionName, parameters, "projects/assignment");
        }
    }
}
