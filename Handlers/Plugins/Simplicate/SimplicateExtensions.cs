using Newtonsoft.Json.Linq;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Handlers.Plugins.Simplicate.Extensions
{
    public static class SimplicateExtensions
    {

        public static JObject ConvertToNestedJson(this Dictionary<string, object> flatDictionary)
        {
            var root = new JObject();

            foreach (var item in flatDictionary)
            {
                var path = item.Key.Split('.');
                JObject currentObject = root;

                for (int i = 0; i < path.Length; i++)
                {
                    if (i == path.Length - 1)
                    {
                        currentObject[path[i]] = JToken.FromObject(item.Value);
                    }
                    else
                    {
                        if (currentObject[path[i]] == null)
                        {
                            currentObject[path[i]] = new JObject();
                        }

                        var nextObject = currentObject[path[i]] as JObject ?? throw new InvalidOperationException($"Expected a JObject at path '{string.Join(".", path[..(i + 1)])}'");
                        currentObject = nextObject;
                    }
                }
            }

            return root;
        }

        public static string ToFilterString(this Dictionary<string, object> parameters, IEnumerable<ParameterAttribute> options)
        {
            return string.Join("&", parameters
                .Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                .Where(kv => options.Any(t => t.Name == kv.Key))
                .Select(kv => new { Options = options.First(t => t.Name == kv.Key), kv.Value })
                .Select(kv => kv.Options.ToFilterStringValue(kv.Value)));
        }

        public static string ToFilterStringValue(this ParameterAttribute attribute, object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return attribute.ParamType switch
            {
                "string" => attribute.Format switch
                {
                    "date-time" => $"q[{attribute.Name}]={value?.ToString()}",
                    _ => $"q[{attribute.Name}]=*{Uri.EscapeDataString(value?.ToString()!)}*",
                },
                "number" => attribute.Name switch
                {
                    "limit" or "offset" => $"{attribute.Name}={value?.ToString()}",
                    _ => $"q[{attribute.Name}]={Uri.EscapeDataString(value?.ToString()!)}",
                },
                _ => string.Empty,
            };
        }

    }
}