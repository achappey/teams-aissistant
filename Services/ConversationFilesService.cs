
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using TeamsAIssistant.AdaptiveCards;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Services
{
  public class ConversationFilesService(FileService fileService, GraphClientServiceProvider graphClientServiceProvider,
    ProactiveMessageService proactiveMessageService, DownloadService downloadService)
  {

    public async Task SaveFile(ITurnContext turnContext, Models.File file)
    {
      if (file.Filename != null && file.Content != null)
      {
        var url = await downloadService.UploadDriveFileAsync(turnContext.Activity.Recipient.Name, file.Filename, file.Content);

        if (url != null)
        {
          FileCardData cardData = new(new(turnContext.Activity.Locale))
          {
            Filename = file.Filename,
            Url = url
          };

          await proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                            () => FileCards.FileCardTemplate.RenderAdaptiveCard(cardData),
                            null, 
                            CancellationToken.None);
        }
      }
    }

    public async Task<Models.File?> AddFileAsync(ITurnContext turnContext, Models.File file)
    {
      var connectionReference = turnContext.Activity.GetConversationReference();

      if (file != null && file.Url != null)
      {

        FileCardData cardData = new(new(turnContext.Activity.Locale))
        {
          Filename = file.Filename,
          Url = file.Url,
          Status = "Reading"
        };

        var uploadCardId = await proactiveMessageService.SendOrUpdateCardAsync(
            connectionReference,
            () => FileCards.FileCardTemplate.RenderAdaptiveCard(cardData),
            null, CancellationToken.None);

        try
        {
          var result = await downloadService.DownloadAttachmentAsync(graphClientServiceProvider.AadObjectId!, file);
          if (result == null || result.Content == null || result.Filename == null)
          {
            await SendCardUpdateAsync("Download error", connectionReference, uploadCardId, file.Filename ?? string.Empty, file.Url, turnContext.Activity.Locale);
            return null;
          }

          var tool = result.Filename.GetToolTypeFromFile();
          if (tool == null)
          {
            await SendCardUpdateAsync("File not supported", connectionReference, uploadCardId, result.Filename, file.Url, turnContext.Activity.Locale);
            return null;
          }

          await SendCardUpdateAsync("Processing", connectionReference, uploadCardId, result.Filename, file.Url, turnContext.Activity.Locale);

          var newFile = await fileService.UploadFileAsync(result.Filename, result.Content);

          if (newFile.Id == null)
          {
            await SendCardUpdateAsync("Something went wrong", connectionReference, uploadCardId, result.Filename, file.Url, turnContext.Activity.Locale);
            return null;
          }

          await SendCardUpdateAsync("Ready", connectionReference, uploadCardId, result.Filename, file.Url, turnContext.Activity.Locale);
          result.Id = newFile.Id;
          return result;
        }
        catch (Exception e)
        {
          await SendCardUpdateAsync(e.Message, connectionReference, uploadCardId, file?.Filename ?? string.Empty, file?.Url ?? string.Empty, turnContext.Activity.Locale);
        }
      }

      return null;
    }

    private Task<string?> SendCardUpdateAsync(string status,
      ConversationReference connectionReference,
      string? uploadCardId,
      string filename,
      string url,
      string locale)
    {
      FileCardData cardData = new(new(locale))
      {
        Filename = filename,
        Url = url,
        Status = status
      };

      return proactiveMessageService.SendOrUpdateCardAsync(
          connectionReference,
          () => FileCards.FileCardTemplate.RenderAdaptiveCard(cardData),
          uploadCardId, CancellationToken.None);
    }
  }
}