using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Attributes;
using Newtonsoft.Json;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphContactsPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Contacts")
    {
        [Action("MicrosoftGraph.ListContacts")]
        [Description("Get contacts in the user's mailbox")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to get the contacts from. Defaults to current user")]
        [Parameter(name: "displayName", type: "string", description: "Name of the contact")]
        [Parameter(name: "companyName", type: "string", description: "Company name of the contact")]
        [Parameter(name: "department", type: "string", description: "Department of the contact")]
        public Task<string> ListContacts([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                        ? await graphClient.Users[value.ToString()].Contacts
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToFilterString();
                                }) : await graphClient.Me.Contacts
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToFilterString();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.ListContactFolders")]
        [Description("Get contact folders in the user's mailbox")]
        [Parameter(name: "userId", type: "string", description: "User id of the user to get the contact folders from. Defaults to current user")]
        public Task<string> ListContactFolders([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
        [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = parameters.TryGetValue("userId", out object? value)
                        ? await graphClient.Users[value.ToString()].ContactFolders
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToFilterString();
                                }) : await graphClient.Me.ContactFolders
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Filter = parameters.ToFilterString();
                                });

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.CreateNewContact")]
        [Description("Creates a new contact in the user's contacts with Microsoft Graph")]
        [Parameter(name: "givenName", type: "string", required: true, description: "Given name of the contact")]
        [Parameter(name: "surName", type: "string", required: true, description: "Surname of the contact")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Display name of the contact")]
        [Parameter(name: "email", type: "string", required: true, description: "Email of the contact")]
        [Parameter(name: "companyName", type: "string", description: "Company name of the contact")]
        [Parameter(name: "jobTitle", type: "string", description: "Job title of the contact")]
        [Parameter(name: "personalNotes", type: "string", multiline: true, description: "Personal notes of the contact")]
        public Task<string> CreateNewContact([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateNewContactSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateNewContact", data, 
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new Contact
                    {
                        GivenName = jObject?["givenName"]?.ToString(),
                        Surname = jObject?["surName"]?.ToString(),
                        JobTitle = jObject?["jobTitle"]?.ToString(),
                        CompanyName = jObject?["companyName"]?.ToString(),
                        PersonalNotes = jObject?["personalNotes"]?.ToString(),
                        DisplayName = jObject?["displayName"]?.ToString(),
                        EmailAddresses =
                        [
                            new() {
                                Address = jObject?["email"]?.ToString(),
                                Name = jObject?["displayName"]?.ToString(),
                            },
                        ],
                    };

                    var result = await graphClient.Me.Contacts.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(result);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.DeleteContact")]
        [Description("Deletes a contact")]
        [Parameter(name: "contactId", type: "string", required: true, visible: false, description: "Id of the contact")]
        public Task<string> DeleteContact([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var contactId = parameters["contactId"]?.ToString();

                    var contact = await graphClient.Me.Contacts[contactId].GetAsync();

                    var displayName = contact?.DisplayName ?? string.Empty;
                    var mail = contact?.EmailAddresses?.FirstOrDefault()?.Address ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "DisplayName", type: "string", readOnly: true), displayName),
                        (new ParameterAttribute(name: "Mail", type: "string", readOnly: true), mail)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteContactSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteContact", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.Contacts[jObject?["contactId"]?.ToString()].DeleteAsync();

                    return "Contact deleted";
                }, cancellationToken);
        }

    }
}
