using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Services
{
  public class FileService(FileRepository fileRepository)
  {
    public Task<bool> DeleteFileAsync(string fileId)
    {
      return fileRepository.DeleteFileAsync(fileId);
    }

    public Task<Models.File> UploadFileAsync(string filename, byte[] file)
    {
      return fileRepository.UploadFileAsync(filename, file);
    }

    public Task<Models.File> GetFileAsync(string fileId)
    {
      return fileRepository.GetFileAsync(fileId);
    }

  }
}