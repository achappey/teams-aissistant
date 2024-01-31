
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Threads;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Models;

namespace TeamsAIssistant.Services
{

  public class AssistantRepository(OpenAIClient openAIDotNet)
  {
    private readonly OpenAIClient _openAIDotNet = openAIDotNet;

    public async Task<bool> DeleteAssistantFileAsync(string assistantId, string fileId)
    {
      var assistantResponse = await _openAIDotNet.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);
      var response = await assistantResponse.DeleteFileAsync(fileId);

      return response;
    }

    public Task<bool> DeleteAssistantAsync(string assistantId)
    {
      return _openAIDotNet.AssistantsEndpoint.DeleteAssistantAsync(assistantId);
    }

    public async Task<IEnumerable<Models.Message>> GetThreadMessagesAsync(string threadId)
    {
      List<Models.Message> allMessages = [];
      string? lastId = null;
      bool hasMore = true;

      while (hasMore)
      {
        var query = new ListQuery() { Limit = 100 };
        if (!string.IsNullOrEmpty(lastId))
        {
          query.After = lastId;
        }

        var response = await _openAIDotNet.ThreadsEndpoint.ListMessagesAsync(threadId, query);

        if (response.Items != null && response.Items.Any())
        {
          allMessages.AddRange(response.Items.Select(t => new Models.Message()
          {
            Id = t.Id,
            CreatedAt = t.CreatedAt,
            Role = t.Role.ToString(),
            Content = t.Content != null && t.Content.Where(r => r.Text != null).Any()
              ? t.Content.Where(r => r.Text != null)?.FirstOrDefault()?.Text?.Value ?? string.Empty
                : string.Empty
          }));
        }

        lastId = response.Items?.LastOrDefault()?.Id;
        hasMore = response.HasMore;
      }

      return allMessages.Where(r => !string.IsNullOrEmpty(r.Content));
    }


    public async Task<Models.File> CreateAssistantFileAsync(string assistantId, string fileId)
    {
      var fileResponse = await _openAIDotNet.FilesEndpoint.GetFileInfoAsync(fileId);
      var response = await _openAIDotNet.AssistantsEndpoint.AttachFileAsync(assistantId, fileResponse);

      return new Models.File()
      {
        Filename = fileResponse.FileName,
        Id = response.Id
      };
    }

    public async Task<Assistant> CreateAssistantAsync(Assistant assistant)
    {
      var updateAssistantRequest = new CreateAssistantRequest(name: assistant.Name,
        description: assistant.Description,
        instructions: assistant.Instructions,
        tools: assistant.Tools?.Select(t => t.ToTool()),
        model: assistant.Model,
        metadata: assistant.Metadata?.ToDictionary(e => e.Key, e => e.Value?.ToString()));

      var response = await _openAIDotNet.AssistantsEndpoint.CreateAssistantAsync(updateAssistantRequest);

      return response.ToAssistant();
    }

    public async Task<Assistant> GetAssistantAsync(string assistantId)
    {
      var response = await _openAIDotNet.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);

      return response.ToAssistant();
    }

    public async Task<Assistant> UpdateAssistantAsync(Assistant assistant)
    {
      var updateAssistantRequest = new CreateAssistantRequest(name: assistant.Name,
        description: assistant.Description ?? string.Empty,
        tools: assistant.Tools?.Select(y => y.ToTool()),
        instructions: assistant.Instructions,
        model: assistant.Model,
        metadata: assistant.Metadata?.ToDictionary(e => e.Key, e => e.Value?.ToString()));

      var response = await _openAIDotNet.AssistantsEndpoint.ModifyAssistantAsync(assistant.Id, updateAssistantRequest);

      return response.ToAssistant();
    }

    public async Task<List<Assistant>> GetAssistantsAsync()
    {
      var response = await _openAIDotNet.AssistantsEndpoint.ListAssistantsAsync();
      var items = response.Items.Select(a => a.ToAssistant());

      return items.ToList();
    }

    private async Task<IEnumerable<RunResponse>> GetAllThreadRunsAsync(string threadId)
    {
      List<RunResponse> allMessages = [];
      string? lastId = null;
      bool hasMore = true;

      while (hasMore)
      {
        var query = new ListQuery() { Limit = 100 };
        if (!string.IsNullOrEmpty(lastId))
        {
          query.After = lastId;
        }

        var response = await _openAIDotNet.ThreadsEndpoint.ListRunsAsync(threadId, query);

        if (response.Items != null && response.Items.Any())
        {
          allMessages.AddRange(response.Items);
        }

        lastId = response.Items?.LastOrDefault()?.Id;
        hasMore = response.HasMore;
      }

      return allMessages;
    }

    public async Task<IEnumerable<(string model, int input, int output)>> GetThreadUsageAsync(string threadId)
    {
      var response = await GetAllThreadRunsAsync(threadId);

      return response
          .GroupBy(run => run.Model)
          .Select(group => (
              model: group.Key,
              input: group.Sum(y => y.Usage?.PromptTokens) ?? 0,
              output: group.Sum(y => y.Usage?.CompletionTokens) ?? 0
          ));
    }

    public async Task<List<(string input, IEnumerable<string> logs)>> GetToolCalls(string threadId, string runId)
    {
      var response = await _openAIDotNet.ThreadsEndpoint.ListRunStepsAsync(threadId, runId);

      return response.Items
        .Where(tc => tc.StepDetails.ToolCalls != null)
        .SelectMany(t => t.StepDetails.ToolCalls
            .Where(tc => tc.CodeInterpreter != null)
            .Where(tc => !string.IsNullOrEmpty(tc.CodeInterpreter.Input))
            .Select(tc => (tc.CodeInterpreter.Input, tc.CodeInterpreter.Outputs.Select(g => g.Logs))))
        .ToList();
    }
  }
}