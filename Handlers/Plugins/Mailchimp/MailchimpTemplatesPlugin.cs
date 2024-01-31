using Microsoft.Bot.Builder;
using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.State;
using System.ComponentModel;
using TeamsAIssistant.Services;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using MailChimp.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Mailchimp
{
    public class MailchimpTemplatesPlugin(MailChimpManager mailChimpManager,
            ProactiveMessageService proactiveMessageService, DriveRepository driveRepository)
                : MailchimpBasePlugin(mailChimpManager, proactiveMessageService, driveRepository, "Templates")
    {
        [Action("Mailchimp.ListTemplates")]
        [Description("Lists available templates in Mailchimp")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        [Parameter(name: "count", type: "number", maximum: 1000, description: "The number of records to return")]
        [Parameter(name: "since_date_created", type: "string", description: "Restrict the response to templates created after the set date. Uses ISO 8601 time format: 2015-10-21T15:41:36+00:00")]
        [Parameter(name: "before_date_created", type: "string", description: "Restrict the response to templates created before the set date")]
        [Parameter(name: "sort_field", type: "string", enumValues: ["DateCreated", "Name"], description: "Returns user templates sorted by the specified field")]
        public Task<string> ListTemplates([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Templates.GetAllAsync(new MailChimp.Net.Core.TemplateRequest()
                {
                    Offset = paramDict.Offset,
                    Count = paramDict.Count,
                    SincedCreatedAt = parameters.TryGetValue("since_date_created", out object? since_date_created)
                                ? DateTime.Parse(since_date_created.ToString()!) : null,
                    BeforeCreatedAt = parameters.TryGetValue("before_date_created", out object? before_date_created)
                                ? DateTime.Parse(before_date_created.ToString()!) : null,
                    SortByField = parameters.TryGetValue("sort_field", out object? sort_field)
                                ? Enum.Parse<MailChimp.Net.Core.TemplateSortField>(sort_field.ToString()!)
                                : MailChimp.Net.Core.TemplateSortField.Name,
                }));
        }

        [Action("Mailchimp.CreateTemplate")]
        [Description("Creates a new Mailchimp template")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the template")]
        [Parameter(name: "html", type: "string", required: true, description: "The raw HTML for the template. We support the Mailchimp Template Language in any HTML code passed via the API")]
        public Task<string> CreateTemplate([ActionTurnContext] TurnContext turnContext,
                                       [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MailchimpCreateTemplateSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "Mailchimp.CreateTemplate", data,
                async (MailChimpManager client, JObject? jObject) =>
                {
                    var name = jObject?["name"]?.ToString();
                    var html = jObject?["html"]?.ToString();
                    var result = await client.Templates.CreateAsync(name: name, folderId: null, html: html);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("Mailchimp.GetTemplateDefaultContent")]
        [Description("Get the sections that you can edit in a template, including each section's default content")]
        [Parameter(name: "template_id", type: "string", required: true, description: "Id of the template")]
        public Task<string> GetTemplateDefaultContent([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.Templates.GetDefaultContentAsync(parameters["template_id"]?.ToString()));
        }

        [Action("Mailchimp.ListTemplateFolders")]
        [Description("Lists template folders in Mailchimp")]
        [Parameter(name: "offset", type: "number", description: "Used for pagination, this it the number of records from a collection to skip")]
        public Task<string> ListTemplateFolders([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteMailchimpQuery(
                turnContext, turnState, actionName, parameters,
                (client, paramDict) => client.TemplateFolders.GetAllAsync(new MailChimp.Net.Core.QueryableBaseRequest()
                {
                    Offset = paramDict.Offset,
                }));
        }

        [Action("Mailchimp.CreateTemplateFolder")]
        [Description("Creates a new Mailchimp template folder")]
        [Parameter(name: "name", type: "string", required: true, description: "Name of the template folder")]
        public Task<string> CreateTemplateFolder([ActionTurnContext] TurnContext turnContext,
                                     [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MailchimpCreateTemplateFolderSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "Mailchimp.CreateTemplateFolder", data,
                async (MailChimpManager client, JObject? jObject) =>
                {
                    var name = jObject?["name"]?.ToString();
                    var result = await client.TemplateFolders.AddAsync(name: name);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }
    }
}
