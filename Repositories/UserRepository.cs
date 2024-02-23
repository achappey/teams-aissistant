using Microsoft.Graph.Beta.Models;
using TeamsAIssistant.Services;

namespace TeamsAIssistant.Repositories
{
  public class UserRepository(GraphClientServiceProvider graphClientServiceProvider)
  {
    public async Task<IEnumerable<string>>? GetUsersByIds(IEnumerable<string> ids)
    {
      var graphClient = graphClientServiceProvider.GetAuthenticatedGraphClient();

      var result = await graphClient.Users.GetByIds.PostAsGetByIdsPostResponseAsync(
          new Microsoft.Graph.Beta.Users.GetByIds.GetByIdsPostRequestBody()
          {
            Ids = ids.ToList()
          });

      return result?.Value?.Select(t => (t as User)!.DisplayName)!.Order()!;
    }

    public async Task<IEnumerable<Site>> GetFollowedSites()
    {
      var graphClient = graphClientServiceProvider.GetAuthenticatedGraphClient();
      var result = await graphClient.Me.FollowedSites.GetAsync();

      return result?.Value ?? [];
    }

    public Task<Site?> GetSite(string siteId)
    {
      var graphClient = graphClientServiceProvider.GetAuthenticatedGraphClient();
      return graphClient.Sites[siteId].GetAsync();
    }

    public async Task<IEnumerable<Team>> GetJoinedTeams()
    {
      var graphClient = graphClientServiceProvider.GetAuthenticatedGraphClient();
      var result = await graphClient.Me.JoinedTeams.GetAsync();

      return result?.Value ?? [];
    }

    public Task<Team?> GetTeam(string teamId)
    {
      var graphClient = graphClientServiceProvider.GetAuthenticatedGraphClient();
      return graphClient.Teams[teamId].GetAsync();
    }
  }
}