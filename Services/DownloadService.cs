using System.Web;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Services;

public class DownloadService(WebRepository webRepository, DriveRepository driveRepository, SimplicateClientServiceProvider simplicateClientServiceProvider)
{
  public Task<string?> UploadDriveFileAsync(string folderName, string filename, byte[] file)
  {
    return driveRepository.UploadDriveFileAsync(folderName, filename, file);
  }

  public Task<Models.File?> DownloadDriveFileAsync(string url)
  {
    return driveRepository.DownloadDriveFileAsync(url);
  }

  public Task<byte[]?> DownloadFileAsync(string url)
  {
    return webRepository.DownloadFileAsync(url);
  }

  public async Task<Models.File?> DownloadAttachmentAsync(string aadObjectId, Models.File attachment)
  {
    if (attachment == null || attachment.Url == null)
    {
      throw new ArgumentNullException(nameof(attachment));
    }

    if (aadObjectId != null)
    {
      if (attachment.Url.Contains(".sharepoint.com"))
      {
        try
        {
          return await driveRepository.DownloadDriveFileAsync(attachment.Url);
        }
        catch (Exception e)
        {
          if (e.Message == "Site Pages cannot be accessed as a drive item")
          {
            return await driveRepository.DownloadSharePointPageAsync(attachment.Url);
          }

          throw;
        }
      }
      else if (attachment.Url.Contains(".simplicate.nl/api/v2/storage/loadfile"))
      {
        var simplicateClient = await simplicateClientServiceProvider.GetAuthenticatedSimplicateClient(aadObjectId);
        using var response = await simplicateClient.GetAsync(attachment.Url);
        var queryParameters = HttpUtility.ParseQueryString(new Uri(attachment.Url).Query);
        var filename = queryParameters["filename"];
        var simplicateByteContent = await response.Content.ReadAsByteArrayAsync();

        return new Models.File()
        {
          Filename = filename ?? string.Empty,
          Content = simplicateByteContent
        };
      }
    }

    var byteContent = await webRepository.DownloadFileAsync(attachment.Url);

    var fileName = attachment.Url.UrlToFileName();

    if (!Path.HasExtension(fileName))
    { 
      fileName += ".html";
    }

    return new Models.File()
    {
      Filename = fileName,
      Content = byteContent
    };


  }
}
