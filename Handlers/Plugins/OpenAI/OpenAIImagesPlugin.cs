using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using OpenAI.Managers;

namespace TeamsAIssistant.Handlers.Plugins.AI
{
    public class OpenAIImagesPlugin(OpenAIService openAIService, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : OpenAIBasePlugin(openAIService, proactiveMessageService, driveRepository, "Images")
    {

        [Action("OpenAI.CreateImage")]
        [Description("Creates an image with OpenAI Dall-E 3")]
        [Parameter(name: "prompt", type: "string", required: true, maxLength: 4000,
            description: "Prompt to create the image. Don't describe where the image is used for, only a very detailed description of how the image should look like")]
        [Parameter(name: "style", type: "string", required: true, enumValues: ["vivid", "natural"],
            description: "The style of the generated images. Must be one of vivid or natural. Vivid causes the model to lean towards generating hyper-real and dramatic images. Natural causes the model to produce more natural, less hyper-real looking images")]
        [Parameter(name: "size", type: "string", required: true, enumValues: ["1024x1024", "1792x1024", "1024x1792"],
            description: "The size of the generated images. Must be one of 1024x1024, 1792x1024, or 1024x1792")]
        public async Task<string> CreateImage([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            var prompt = parameters["prompt"]?.ToString();
            var size = parameters["size"]?.ToString();
            var style = parameters["style"]?.ToString();

            if (prompt == null)
            {
                return "Prompt missing";
            }

            if (size == null)
            {
                return "Size missing";
            }

            if (style == null)
            {
                return "Style missing";
            }

            return await ExecuteOpenAIQuery(
                turnContext, turnState, actionName, parameters,
                async (openaiClient, paramDict) =>
                    {
                        var result = await openaiClient.Image
                            .CreateImage(new OpenAI.ObjectModels.RequestModels.ImageCreateRequest()
                            {
                                Model = "dall-e-3",
                                Quality = "hd",
                                User = turnContext.Activity.From.Name,
                                Prompt = prompt,
                                Size = size,
                                Style = style
                            });

                        foreach (var item in result.Results)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.ContentUrl(item.Url, "image/png"));
                        }

                        return result.Results;
                    });
        }
    }
}
