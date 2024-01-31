using TeamsAIssistant.Config;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Services;

public class SimplicateClientServiceProvider(IHttpClientFactory httpClientFactory, KeyVaultRepository keyVaultRepository, IConfiguration configuration)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly string? _environment = configuration.Get<ConfigOptions>()!.SimplicateVaultName;
    private HttpClient? _httpClient;

    public async Task<HttpClient> GetAuthenticatedSimplicateClient(string aadObjectId)
    {
        if (_httpClient != null)
        {
            return _httpClient;
        }

        if (_environment == null)
        {
            throw new ArgumentException("Simplicate configuration missing");
        }

        _httpClient = _httpClientFactory.CreateClient("SimplicateClient");
        _httpClient.BaseAddress = new Uri($"https://{_environment.Split("-").FirstOrDefault()}.simplicate.nl/api/v2/");

        var secret = await keyVaultRepository.GetSecret(_environment, aadObjectId);

        _httpClient.DefaultRequestHeaders.Add("Authentication-Key", secret.Properties.ContentType);
        _httpClient.DefaultRequestHeaders.Add("Authentication-Secret", secret.Value);

        return _httpClient;
    }
}
