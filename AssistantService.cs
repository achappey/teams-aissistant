using Microsoft.Teams.AI.AI.OpenAI.Models;
using OpenAI;
using OpenAI.Interfaces;
using OpenAI.Managers;

public class AssistantService
{
  private readonly OpenAIClient _openAIDotNet;
  private readonly OpenAIService _openAIBetalgo;

  public AssistantService(OpenAIClient openAIDotNet, OpenAIService openAIBetalgo)
  {
    _openAIDotNet = openAIDotNet;
    _openAIBetalgo = openAIBetalgo;
  }

  public async Task<string?> GetFileContentAsync(string fileId)
  {
    var response = await _openAIBetalgo.Files.RetrieveFileContent(fileId);

    return response.Content;
  }

  public async Task<TeamsAIssistant.Models.File> GetFileAsync(string fileId)
  {
    var response = await _openAIDotNet.FilesEndpoint.GetFileInfoAsync(fileId);

    return new TeamsAIssistant.Models.File()
    {
      Filename = response.FileName,
      CreatedAt = response.CreatedAt,
      Bytes = response.Size,
      Id = response.Id,
    };
  }

  public async Task<Assistant> GetAssistantAsync(string assistantId)
  {
    var response = await _openAIDotNet.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);

    return new Assistant()
    {
      Id = assistantId,
      Name = response.Name,
      Model = response.Model,
      CreatedAt = response.CreatedAt.ToFileTimeUtc(),
      Description = response.Description,
      FileIds = response.FileIds.ToList(),
      Instructions = response.Instructions
    };
  }
}
