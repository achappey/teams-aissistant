using Microsoft.Teams.AI.AI.OpenAI.Models;
using TeamsAIssistant.Config;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Models;

namespace TeamsAIssistant.Services
{
  public class AssistantService(AssistantRepository assistantRepository, IConfiguration configuration, FileService fileService, UserService? userService = null)
  {
    private readonly string _defaultAssistantId = configuration.Get<ConfigOptions>()!.OpenAI!.AssistantId!;

    public Task<bool> DeleteAssistantFileAsync(string fileId, string? assistantId = null)
    {
      return assistantRepository.DeleteAssistantFileAsync(assistantId ?? _defaultAssistantId, fileId);
    }

    public async Task<bool> DeleteAssistantAsync(string assistantId)
    {
      var assistant = await assistantRepository.GetAssistantAsync(assistantId);

      foreach (var file in assistant.FileIds)
      {
        await fileService.DeleteFileAsync(file);
      }

      return await assistantRepository.DeleteAssistantAsync(assistantId);
    }

    public Task<Models.File> CreateAssistantFileAsync(string fileId, string? assistantId = null)
    {
      return assistantRepository.CreateAssistantFileAsync(assistantId ?? _defaultAssistantId, fileId);
    }

    public async Task<Assistant> CloneAssistantAsync(string currentUserId, string assistantId)
    {
      var assistant = await GetAssistantAsync(assistantId);

      assistant.Name += new Random().Next(1000);
      assistant.Metadata = assistant.Metadata?
        .WithOwner(currentUserId)
        .WithVisibility(Enum.GetName(Visibility.Owners));

      return await assistantRepository.CreateAssistantAsync(assistant);
    }

    public bool IsDefaultAssistant(string assistantId)
    {
      return assistantId == _defaultAssistantId;
    }

    public Task<IEnumerable<(string model, int input, int output)>> GetThreadUsageAsync(string threadId)
    {
      return assistantRepository.GetThreadUsageAsync(threadId);
    }

    public Task<Assistant> GetAssistantAsync(string? assistantId = null)
    {
      return assistantRepository.GetAssistantAsync(!string.IsNullOrEmpty(assistantId) ? assistantId : _defaultAssistantId);
    }

    public Task<Assistant> UpdateAssistantAsync(Assistant assistant)
    {
      return assistantRepository.UpdateAssistantAsync(assistant);
    }

    public Task<IEnumerable<Message>> GetLastMessages(string threadId, int items = 5)
    {
      return assistantRepository.GetLastMessages(threadId, items);
    }

    public Task<IEnumerable<Message>> GetThreadMessagesAsync(string threadId)
    {
      return assistantRepository.GetThreadMessagesAsync(threadId);
    }

    public async Task<IEnumerable<Assistant>> GetAssistantsAsync(string currentUserId)
    {
      if (string.IsNullOrEmpty(currentUserId))
      {
        return [];
      }

      var assistants = await assistantRepository.GetAssistantsAsync();
      var joinedTeams = userService != null ? await userService.GetJoinedTeams(currentUserId) : [];
      var teamIds = joinedTeams.Select(t => t.Id!).ToArray();

      return assistants.Where(t =>
           t.Metadata == null ||
           !t.Metadata.ContainsKey(AssistantMetadata.Visibility) &&
           !t.Metadata.ContainsKey(AssistantMetadata.Owners) ||
           !t.Metadata.ContainsKey(AssistantMetadata.Visibility) &&
           t.IsOwner(currentUserId) ||
           t.Metadata.ContainsKey(AssistantMetadata.Visibility) &&
            (t.Metadata[AssistantMetadata.Visibility].ToString() == Enum.GetName(Visibility.Organization) ||
              t.IsOwner(currentUserId)) ||
             (t.Metadata.ContainsKey(AssistantMetadata.Team)
              && t.Metadata[AssistantMetadata.Visibility].ToString() == Enum.GetName(Visibility.Team) &&
              t.IsTeamMember(teamIds))
           ).OrderBy(t => t.Name);
    }

    public Task<List<(string input, IEnumerable<string> logs)>> GetToolCalls(string threadId, string runId)
    {
      return assistantRepository.GetToolCalls(threadId, runId);
    }
  }
}