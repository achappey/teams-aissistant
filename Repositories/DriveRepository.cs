using System.Text;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Services;

namespace TeamsAIssistant.Repositories
{
  public class DriveRepository(GraphClientServiceProvider graphClientServiceProvider)
  {
    private readonly GraphClientServiceProvider _graphClientServiceProvider = graphClientServiceProvider;

    public async Task<Models.File?> DownloadDriveFileAsync(string url)
    {
      var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
      var result = await graphClient.GetDriveItem(url);

      if (result?.ParentReference?.DriveId != null && result.Id != null)
      {
        var bytes = await graphClient.GetDriveItemContent(result.ParentReference.DriveId, result.Id);

        return new Models.File()
        {
          Filename = result.Name ?? string.Empty,
          Content = bytes
        };
      }

      return null;
    }

    public async Task<Models.File?> DownloadSharePointPageAsync(string url)
    {
      var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
      var (Hostname, Path, PageName) = url.ExtractSharePointValues();

      var site = await graphClient.Sites[$"{Hostname}:/sites/{Path}"].GetAsync();
      var pages = await graphClient.Sites[site?.Id].Pages.GraphSitePage.GetAsync((config) =>
      {
        config.QueryParameters.Select = ["name", "id", "title", "webUrl", "createdDateTime", "createdBy"];
        config.QueryParameters.Top = 999;
      });

      var page = pages?.Value?.FirstOrDefault(t => t.Name == PageName);

      var sitePage = await graphClient.Sites[site?.Id].Pages[page?.Id].GraphSitePage.GetAsync((config) =>
      {
        config.QueryParameters.Expand = ["canvasLayout"];
      });

      var allInnerHtml = sitePage?.CanvasLayout?.HorizontalSections?
                .SelectMany(hs => hs.Columns ?? [])
                .SelectMany(c => c.Webparts ?? [])
                .OfType<TextWebPart>()
                .Select(wp => wp.InnerHtml)
                .ToList();

      if (sitePage?.CanvasLayout?.VerticalSection != null)
      {
        allInnerHtml?.AddRange(sitePage?.CanvasLayout?.VerticalSection?.Webparts?
            .OfType<TextWebPart>()
            .Select(wp => wp.InnerHtml) ?? []);
      }

      var html = string.Join("", allInnerHtml?.Where(y => !string.IsNullOrEmpty(y)) ?? []);

      if (!string.IsNullOrEmpty(html))
      {
        var htmlString = @$"<html><head><meta name='author' content='{page?.CreatedBy?.User?.DisplayName}'>
          <meta name='creation-date' content='{page?.CreatedDateTime}'>
          <meta name='source-url' content='{page?.WebUrl}'>
          <title>{page?.Title}</title>
          </head>
          <body>
          <div>{html}</div>
          </body>
          </html>";

        return new Models.File()
        {
          Filename = $"{page?.Title}.html" ?? string.Empty,
          Content = Encoding.UTF8.GetBytes(htmlString)
        };
      }

      return null;
    }

    public async Task<string?> UploadDriveFileAsync(string folderName, string filename, byte[] file)
    {
      var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
      await using MemoryStream memoryStream = new(file);
      var userDrive = await graphClient.Me.Drive.GetAsync();
      var result = await graphClient.Drives[userDrive?.Id].Items["root"].ItemWithPath($"/{folderName}/{filename}").Content.PutAsync(memoryStream);

      return result?.WebUrl;
    }

  }
}