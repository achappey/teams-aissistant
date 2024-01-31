using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph.Beta.Models;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Services
{
  public class UserService(UserRepository userRepository, IMemoryCache memoryCache)
  {
    public Task<IEnumerable<string>>? GetUsersByIds(IEnumerable<string> userIds)
    {
      return userRepository.GetUsersByIds(userIds);
    }

    public async Task<IEnumerable<Team>> GetJoinedTeams(string currentUserId)
    {
      if (memoryCache.TryGetValue(currentUserId, out IEnumerable<Team>? cachedTeams))
      {
        return cachedTeams ?? [];
      }

      var joinedTeams = await userRepository.GetJoinedTeams();
      var cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromHours(1));

      memoryCache.Set(currentUserId, joinedTeams, cacheEntryOptions);

      return joinedTeams;
    }

    public Task<Team?> GetTeam(string teamId)
    {
      return userRepository.GetTeam(teamId);
    }
  }
}