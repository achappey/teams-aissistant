using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using Microsoft.Bot.Schema;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphUserPhotoPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "User Photo")
    {

        [Action("MicrosoftGraph.GetUserPhoto")]
        [Description("Gets the photo of a user")]
        [Parameter(name: "userId", type: "string", description: "Id of the user. Defaults to current user")]
        public Task<string> GetUserPhoto([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                    {
                        var metadata = parameters.TryGetValue("userId", out object? meta)
                          ? await graphClient.Users[meta.ToString()].Photo.GetAsync()
                              : await graphClient.Me.Photo.GetAsync();

                        var contentType = metadata?.AdditionalData["@odata.mediaContentType"]?.ToString();

                        var result = parameters.TryGetValue("userId", out object? value)
                            ? await graphClient.Users[value.ToString()].Photo.Content.GetAsync()
                                : await graphClient.Me.Photo.Content.GetAsync();

                        using MemoryStream memoryStream = new();

                        result?.CopyTo(memoryStream);

                        IMessageActivity imageMessage = MessageFactory.Text(null);
                        imageMessage.Attachments = [
                            new() {
                                    ContentType = contentType,
                                    ContentUrl = $"data:{contentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}"
                                }
                        ];

                        await turnContext.SendActivityAsync(imageMessage);

                        return new
                        {
                            result = $"An adaptive card with the users' photo has been presented",
                            metadata?.Width,
                            metadata?.Height
                        };
                    });
        }

    }
}
