using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphUserManagementPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "User Management")
    {
        [Action("MicrosoftGraph.CreateNewUser")]
        [Description("Creates a new user with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", required: true, description: "Display name of the user")]
        [Parameter(name: "mailNickname", type: "string", required: true, description: "Mail alias of the user, without @")]
        [Parameter(name: "accountEnabled", type: "boolean", required: true, description: "Account enabled")]
        [Parameter(name: "forceChangePasswordNextSignIn", required: true, type: "boolean", description: "Force change password on next sign in")]
        [Parameter(name: "password", type: "string", required: true, format: "password", description: "Password")]
        [Parameter(name: "userPrincipalName", type: "string", required: true, description: "The user principal name (someuser@contoso.com)")]
        public Task<string> CreateNewUser([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphCreateNewUserSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateNewUser", data,
                async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new User
                    {
                        AccountEnabled = jObject?["accountEnabled"]?.ToObject<bool>(),
                        DisplayName = jObject?["displayName"]?.ToString(),
                        MailNickname = jObject?["mailNickname"]?.ToString(),
                        UserPrincipalName = jObject?["userPrincipalName"]?.ToString(),
                        PasswordProfile = new PasswordProfile
                        {
                            ForceChangePasswordNextSignIn = jObject?["forceChangePasswordNextSignIn"]?.ToObject<bool>(),
                            Password = jObject?["password"]?.ToString(),
                        },
                    };

                    var user = await graphClient.Users.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(user);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.UpdateUser")]
        [Description("Updates a user with Microsoft Graph")]
        [Parameter(name: "id", type: "string", required: true, visible: false, readOnly: true, description: "Id of the user")]
        [Parameter(name: "displayName", type: "string", description: "Display name of the user")]
        [Parameter(name: "department", type: "string", description: "Department of the user")]
        [Parameter(name: "mobilePhone", type: "string", description: "Mobile phone of the user")]
        [Parameter(name: "officeLocation", type: "string", description: "Office location of the user")]
        [Parameter(name: "employeeId", type: "string", description: "Employee id of the user")]
        [Parameter(name: "companyName", type: "string", description: "Company name of the user")]
        [Parameter(name: "city", type: "string", description: "City of the user")]
        [Parameter(name: "jobTitle", type: "string", description: "Job title of the user")]
        [Parameter(name: "accountEnabled", type: "boolean", description: "Account enabled")]
        public Task<string> UpdateUser([ActionTurnContext] TurnContext turnContext,
           [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendConfirmationCard(turnContext, actionName, parameters);
        }

        [Submit]
        public Task MicrosoftGraphUpdateUserSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.UpdateUser", data,
            async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var currentUser = await graphClient.Users[jObject?["id"]?.ToString()].GetAsync();

                    var requestBody = new User
                    {
                        AccountEnabled = jObject?.ContainsKey("accountEnabled") == true ? jObject["accountEnabled"]?.ToObject<bool>() : currentUser?.AccountEnabled,
                        DisplayName = jObject?.ContainsKey("displayName") == true ? jObject["displayName"]?.ToString() : currentUser?.DisplayName,
                        Department = jObject?.ContainsKey("department") == true ? jObject["department"]?.ToString() : currentUser?.Department,
                        MobilePhone = jObject?.ContainsKey("mobilePhone") == true ? jObject["mobilePhone"]?.ToString() : currentUser?.MobilePhone,
                        JobTitle = jObject?.ContainsKey("jobTitle") == true ? jObject["jobTitle"]?.ToString() : currentUser?.JobTitle,
                        EmployeeId = jObject?.ContainsKey("employeeId") == true ? jObject["employeeId"]?.ToString() : currentUser?.EmployeeId,
                        CompanyName = jObject?.ContainsKey("companyName") == true ? jObject["companyName"]?.ToString() : currentUser?.CompanyName,
                        City = jObject?.ContainsKey("city") == true ? jObject["city"]?.ToString() : currentUser?.City,
                        OfficeLocation = jObject?.ContainsKey("officeLocation") == true ? jObject["officeLocation"]?.ToString() : currentUser?.OfficeLocation,
                    };

                    var updatedUser = await graphClient.Users.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(updatedUser);
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.GetDeletedUsers")]
        [Description("Gets deleted users with Microsoft Graph")]
        [Parameter(name: "displayName", type: "string", description: "Name of the user")]
        public Task<string> GetDeletedUsers([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Directory.DeletedItems.GraphUser
                            .GetAsync((requestConfiguration) =>
                                {
                                    requestConfiguration.QueryParameters.Search = parameters.ToGraphUserSearchString();
                                });

                        return result?.Value;
                    });
        }
    }
}
