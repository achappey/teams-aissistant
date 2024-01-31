using System.Net.Http.Headers;
using Microsoft.Graph.Beta;

namespace TeamsAIssistant.Services;

public class GraphClientServiceProvider(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private string? _token;

    public void SetToken(string? token)
    {
        _token = token;
    }

    public GraphServiceClient GetAuthenticatedGraphClient()
    {
        var httpClient = _httpClientFactory.CreateClient("AuthenticatedWebClient");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");
        httpClient.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"W. Europe Standard Time\"");
        httpClient.DefaultRequestHeaders.Add("Prefer", "outlook.body-content-type=\"text\"");

        return new GraphServiceClient(httpClient);
    }
}
