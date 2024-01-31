using Microsoft.Teams.AI.AI.Action;
using TeamsAIssistant.Services;
using TeamsAIssistant.State;
using System.ComponentModel;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Extensions;
using Microsoft.Graph.Beta;
using Newtonsoft.Json.Linq;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.Graph
{
    public class GraphSitePagesPlugin(GraphClientServiceProvider graphClientServiceProvider, ProactiveMessageService proactiveMessageService,
        DriveRepository driveRepository) : GraphBasePlugin(graphClientServiceProvider, proactiveMessageService, driveRepository, "Site Pages")
    {
        [Action("MicrosoftGraph.GetSitePages")]
        [Description("Gets the site pages by site id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        [Parameter(name: "top", type: "number", description: "Number of items")]
        [Parameter(name: "skip", type: "number", description: "Number of items to skip")]
        public Task<string> GetSitePages([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                async (graphClient, paramDict) =>
                 {
                     var result = await graphClient.Sites[parameters["siteId"].ToString()].Pages.GraphSitePage
                         .GetAsync((requestConfiguration) =>
                             {
                                 requestConfiguration.QueryParameters.Orderby = ["lastModifiedDateTime desc"];
                                 requestConfiguration.QueryParameters.Top = parameters.GetTop();
                                 requestConfiguration.QueryParameters.Skip = parameters.GetSkip();
                             });

                     return result?.Value;
                 });
        }

        [Action("MicrosoftGraph.GetSitePage")]
        [Description("Gets a site page with content by site and page id with Microsoft Graph")]
        [Parameter(name: "siteId", type: "string", required: true, description: "Id of the site")]
        [Parameter(name: "pageId", type: "string", required: true, description: "Id of the page")]
        public Task<string> GetSitePage([ActionTurnContext] TurnContext turnContext, [ActionTurnState] TeamsAIssistantState turnState,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return ExecuteGraphQuery(
                turnContext, turnState, actionName, parameters,
                (graphClient, paramDict) => graphClient.Sites[parameters["siteId"].ToString()]
                    .Pages[parameters["pageId"].ToString()].GraphSitePage.GetAsync((config) =>
                {
                    config.QueryParameters.Expand = ["canvasLayout"];
                }));
        }

        [Action("MicrosoftGraph.DeleteSitePage")]
        [Description("Deletes a site page")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "pageId", type: "string", required: true, visible: false, description: "Id of the page")]
        public Task<string> DeleteSitePage([ActionTurnContext] TurnContext turnContext,
            [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                   async (GraphServiceClient graphClient) =>
                   {
                       var siteId = parameters["siteId"]?.ToString();
                       var pageId = parameters["pageId"]?.ToString();

                       var site = await graphClient.Sites[siteId].GetAsync();
                       var page = await graphClient.Sites[siteId].Pages[pageId].GetAsync();

                       var pageName = page?.Title ?? string.Empty;
                       var siteName = site?.DisplayName ?? string.Empty;

                       return [
                        (new ParameterAttribute(name: "Site", type: "string", readOnly: true), siteName),
                        (new ParameterAttribute(name: "Page", type: "string", readOnly: true), pageName)
                       ];
                   });
        }

        [Submit]
        public Task MicrosoftGraphDeleteSitePageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.DeleteSitePage", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    await graphClient.Sites[jObject?["siteId"]?.ToString()]
                        .Pages[jObject?["pageId"]?.ToString()].DeleteAsync();

                    return "Page deleted";
                }, cancellationToken);
        }

        [Action("MicrosoftGraph.CreateSitePage")]
        [Description("Creates a new site page")]
        [Parameter(name: "siteId", type: "string", required: true, visible: false, description: "Id of the site")]
        [Parameter(name: "pageTitle", type: "string", required: true, description: "Title of the page")]
        [Parameter(name: "content", type: "string", required: true, description: "Html content of the page")]
        public Task<string> CreateSitePage([ActionTurnContext] TurnContext turnContext,
          [ActionName] string actionName, [ActionParameters] Dictionary<string, object> parameters)
        {
            return SendGraphConfirmationCard(turnContext, actionName, parameters,
                   async (GraphServiceClient graphClient) =>
                   {
                       var siteId = parameters["siteId"]?.ToString();
                       var site = await graphClient.Sites[siteId].GetAsync();
                       var siteName = site?.DisplayName ?? string.Empty;

                       return [
                        (new ParameterAttribute(name: "Site", type: "string", readOnly: true), siteName)
                       ];
                   });
        }

        [Submit]
        public Task MicrosoftGraphCreateSitePageSubmit(ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken)
        {
            return SubmitActionAsync(turnContext, turnState, "MicrosoftGraph.CreateSitePage", data,
              async (GraphServiceClient graphClient, JObject? jObject) =>
                {
                    var requestBody = new SitePage
                    {
                        OdataType = "#microsoft.graph.sitePage",
                        Name = $"{jObject?["pageTitle"]?.ToString()}.aspx",
                        Title = jObject?["pageTitle"]?.ToString(),
                        PageLayout = PageLayoutType.Article,
                        ShowComments = false,
                        ShowRecommendedPages = false,
                  /*      TitleArea = new TitleArea
                        {
                            EnableGradientEffect = true,
                            ImageWebUrl = "/_LAYOUTS/IMAGES/VISUALTEMPLATETITLEIMAGE.JPG",
                            Layout = TitleAreaLayoutType.ColorBlock,
                            ShowAuthor = true,
                            ShowPublishedDate = false,
                            ShowTextBlockAboveTitle = false,
                            TextAboveTitle = "TEXT ABOVE TITLE",
                            TextAlignment = TitleAreaTextAlignmentType.Left,
                            AdditionalData = new Dictionary<string, object>
        {
            {
                "imageSourceType" , 2
            },
            {
                "title" , "sample1"
            },
        },
                        },*/
                        CanvasLayout = new CanvasLayout
                        {
                            HorizontalSections =
                            [
                                new() {
                                    Layout = HorizontalSectionLayoutType.OneColumn,
                                    Id = "1",
                                    Emphasis = SectionEmphasisType.None,
                                    Columns =
                                    [
                                        new HorizontalSectionColumn
                                        {
                                            Id = "1",
                                            Width = 8,
                                            Webparts =
                                            [
                                                new() {
                                                    Id = "6f9230af-2a98-4952-b205-9ede4f9ef548",
                                                    AdditionalData = new Dictionary<string, object>
                                                    {
                                                        {
                                                            "innerHtml" , jObject?["content"]?.ToString() ?? string.Empty
                                                        },
                                                    },
                                                },
                                            ],
                                        },

                                    ],
                                },
                            ],
                        },
                    };


                    var page = await graphClient.Sites[jObject?["siteId"]?.ToString()]
                        .Pages.PostAsync(requestBody);

                    return JsonConvert.SerializeObject(page);
                }, cancellationToken);
        }

    }
}
