using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using TeamsAIssistant.Handlers.Plugins;
using TeamsAIssistant.Models;

namespace TeamsAIssistant.Services
{
  public class PluginService(IServiceProvider serviceProvider, IMemoryCache memoryCache)
  {
    public IEnumerable<string>? GetPluginNames()
    {
      return GetPlugins()?.Select(f => f.Name);
    }

    public IEnumerable<Plugin>? GetPlugins()
    {
      return memoryCache.GetOrCreate("PluginList", entry =>
      {
        entry.SlidingExpiration = TimeSpan.FromMinutes(60);

        var pluginTypes = memoryCache.GetOrCreate("PluginTypes", typeEntry =>
      {
        typeEntry.SlidingExpiration = TimeSpan.FromHours(24);
        return Assembly.GetExecutingAssembly().GetTypes()
          .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginBase)))
          .ToList();
      });

        var result = new List<Plugin>(pluginTypes!.Count);

        foreach (var type in pluginTypes)
        {
          if (serviceProvider.GetService(type) is PluginBase pluginInstance)
          {
            var plugin = pluginInstance.GetPlugin();

            if (!string.IsNullOrEmpty(plugin.Name) && plugin.Actions != null && plugin.Actions.Any())
            {
              result.Add(plugin);
            }
          }
        }

        result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        return result;
      });
    }

    public IEnumerable<Tool> GetPluginTools(string name)
    {
      return GetPlugins()?.FirstOrDefault(f => f.Name == name)?.Actions?.Select(r => r.Tool) ?? [];
    }
  }
}