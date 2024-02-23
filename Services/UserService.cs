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

    public async Task<IEnumerable<Site>> GetFollowedSites(string currentUserId)
    {
      var cacheId = $"{currentUserId}Sites";
      
      if (memoryCache.TryGetValue(cacheId, out IEnumerable<Site>? cachedTeams))
      {
        return cachedTeams ?? [];
      }

      var joinedTeams = await userRepository.GetFollowedSites();
      var cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromHours(1));

      memoryCache.Set(cacheId, joinedTeams, cacheEntryOptions);

      return joinedTeams.OrderBy(r => r.DisplayName);
    }

    public async Task<IEnumerable<Site>> GetSites(IEnumerable<string> siteIds)
    {
      List<Site> sites = [];

      foreach (var siteId in siteIds)
      {
        var site = await userRepository.GetSite(siteId);
        
        if (site != null)
        {
          sites.Add(site);
        }
      }

      return sites;
    }
    
    public async Task<IEnumerable<Team>> GetTeams(IEnumerable<string> teamIds)
    {
      List<Team> teams = [];

      foreach (var id in teamIds)
      {
        var team = await userRepository.GetTeam(id);
        
        if (team != null)
        {
          teams.Add(team);
        }
      }

      return teams;
    }


    public async Task<IEnumerable<Team>> GetJoinedTeams(string currentUserId)
    {
      var cacheName = $"{currentUserId}Teams";
      if (memoryCache.TryGetValue(cacheName, out IEnumerable<Team>? cachedTeams))
      {
        return cachedTeams ?? [];
      }

      var joinedTeams = await userRepository.GetJoinedTeams();
      var cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromHours(1));

      memoryCache.Set(cacheName, joinedTeams, cacheEntryOptions);

      return joinedTeams.OrderBy(r => r.DisplayName);
    }

    public Task<Team?> GetTeam(string teamId)
    {
      return userRepository.GetTeam(teamId);
    }
  }
}