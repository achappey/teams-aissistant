using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphMailRulesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Rules")
    {
        [Action("MicrosoftGraph.ListMyMailFolderRules")]
        [Description("List my mail rules from a folder in Outlook")]
        [Parameter(name: "mailFolderId", type: "string", required: true, visible: false, description: "Id of the mail folder")]
        public Task<string> ListMyMailFolderRules([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.MailFolders[parameters["mailFolderId"]?.ToString()].MessageRules
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteMyMailFolderRule")]
        [Description("Deletes a folder mail rule")]
        [Parameter(name: "mailFolderId", type: "string", required: true, visible: false, description: "Id of the mail folder")]
        [Parameter(name: "ruleId", type: "string", required: true, visible: false, description: "Id of the rule")]
        public Task<string> DeleteMyMailFolderRule([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var result = await graphClient.Me.MailFolders[parameters["mailFolderId"]?.ToString()].MessageRules[parameters["ruleId"]?.ToString()]
                           .GetAsync();

                    var name = result?.DisplayName ?? string.Empty;
                    var enabled = result?.IsEnabled?.ToString() ?? string.Empty;

                    return [
                        (new ParameterAttribute(name: "Name", type: "string", readOnly: true), name),
                        (new ParameterAttribute(name: "Enabled", type: "string", readOnly: true), enabled)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteMyMailFolderRuleSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteMyMailFolderRule", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.MailFolders[jObject?["mailFolderId"]?.ToString()].MessageRules[jObject?["ruleId"]?.ToString()].DeleteAsync();

                    return "Mail rule deleted";
                }, cancellationToken);
        }
    }
}
