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
    public class GraphMailFocusedInboxPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Mail Focused Inbox")
    {
        [Action("MicrosoftGraph.ListMyFocusedInboxOverrides")]
        [Description("List my focused inbox overrides from Outlook")]
        public Task<string> ListMyFocusedInboxOverrides([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var result = await graphClient.Me.InferenceClassification.Overrides
                            .GetAsync();

                        return result?.Value;
                    });
        }

        [Action("MicrosoftGraph.DeleteMyFocusedInboxOverride")]
        [Description("Deletes a focused inbox override")]
        [Parameter(name: "overrideId", type: "string", required: true, visible: false, description: "Id of the override")]
        public Task<string> DeleteMyFocusedInboxOverride([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                async (GraphServiceClient graphClient) =>
                {
                    var overrideId = parameters["overrideId"]?.ToString();
                    var result = await graphClient.Me.InferenceClassification.Overrides[overrideId]
                           .GetAsync();

                    var mail = result?.SenderEmailAddress?.Address ?? string.Empty;
                    var classifyAs = result?.ClassifyAs != null ? Enum.GetName(result.ClassifyAs.Value)! : string.Empty;

                    return [
                        (new ParameterAttribute(name: "Mail", type: "string", readOnly: true), mail),
                        (new ParameterAttribute(name: "Classify as", type: "string", readOnly: true), classifyAs)
                    ];
                });
        }

        [Submit]
        public Task MicrosoftGraphDeleteMyFocusedInboxOverrideSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteMyFocusedInboxOverride", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Me.InferenceClassification.Overrides[jObject?["overrideId"]?.ToString()].DeleteAsync();

                    return "Override deleted";
                }, cancellationToken);
        }


    }
}
