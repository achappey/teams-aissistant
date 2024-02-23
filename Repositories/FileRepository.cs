using OpenAI.Managers;

namespace TeamsAIssistant.Repositories
{
  public class FileRepository(OpenAIService openAIBetalgo)
  {
    public async Task<bool> DeleteFileAsync(string fileId)
    {
      var response = await openAIBetalgo.Files.DeleteFile(fileId);

      return response.Deleted;
    }

    public async Task<Models.File> UploadFileAsync(string filename, byte[] file)
    {
      var response = await openAIBetalgo.Files.UploadFile("assistants", file, filename);

      if (response.Error != null)
      {
        throw new HttpRequestException(response.Error.Message);
      }

      return new Models.File()
      {
        Filename = filename,
        Id = response.Id
      };
    }

    public async Task<Models.File> GetFileAsync(string fileId)
    {
      var response = await openAIBetalgo.Files.RetrieveFile(fileId);

      return new Models.File()
      {
        Filename = response.FileName,
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(response.CreatedAt).LocalDateTime,
        Bytes = response.Bytes,
        Id = response.Id,
      };
    }

  }
}