
using System.Web;
using Microsoft.Teams.AI;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Extensions
{
    public static class HttpExtensions
    {
        public static HttpClient GetDefaultClient(this TeamsAdapter teamsAdapter, string url, string name)
        {
            var client = teamsAdapter.HttpClientFactory.CreateClient(name);
            client.DefaultRequestHeaders.Add("User-Agent", AIConstants.AIUserAgent);
            client.BaseAddress = new Uri(url);

            return client;
        }

        public static string GetFullUrl(this HttpClient client, string url, Dictionary<string, object> parameters, string[]? excludeKeys = null)
        {
            if (client.BaseAddress == null)
            {
                throw new InvalidOperationException("The HttpClient's BaseAddress property cannot be null when constructing a full URL.");
            }

            var uriBuilder = new UriBuilder(new Uri(client.BaseAddress, url))
            {
                Query = parameters.ToQueryString(excludeKeys)
            };

            return uriBuilder.ToString();
        }

        public static string? ToQueryString(this Dictionary<string, object> parameters, string[]? excludeKeys = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var param in parameters)
            {
                if (excludeKeys == null || !excludeKeys.Contains(param.Key))
                {
                    query[param.Key] = param.Value.ToString();
                }
            }

            return query.ToString();
        }

        public static Task<string> GetHttpResponseResult(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync();
            }

            return Task.FromResult(response.ReasonPhrase ?? AIConstants.AIUnknownErrorMessage);
        }

        public static string ToFilterString(this Dictionary<string, object> parameters, IEnumerable<string>? exclude = null)
        {
            return string.Join("&", parameters
                .Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                .Where(g => exclude == null || !exclude.Contains(g.Key))
                .Select(t => $"{t.Key}={t.Value}"));
        }
    }
}