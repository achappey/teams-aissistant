using Microsoft.Graph.Beta.Models;
using TeamsAIssistant.Services;

namespace TeamsAIssistant.Repositories;

public class UserRepository(GraphClientServiceProvider graphClientServiceProvider)
{
  private readonly GraphClientServiceProvider _graphClientServiceProvider = graphClientServiceProvider;

  public async Task<IEnumerable<string>>? GetUsersByIds(IEnumerable<string> ids)
  {
    var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
    var result = await graphClient.Users.GetByIds.PostAsGetByIdsPostResponseAsync(new Microsoft.Graph.Beta.Users.GetByIds.GetByIdsPostRequestBody()
    {
      Ids = ids.ToList()
    });

    return result?.Value?.Select(t => (t as User)!.DisplayName)!.Order()!;
  }

  public async Task<IEnumerable<Team>> GetJoinedTeams()
  {
    var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
    var result = await graphClient.Me.JoinedTeams.GetAsync();

    return result?.Value ?? [];
  }

  public async Task<Team?> GetTeam(string teamId)
  {
    var graphClient = _graphClientServiceProvider.GetAuthenticatedGraphClient();
    var result = await graphClient.Teams[teamId].GetAsync();

    return result;
  }
}
