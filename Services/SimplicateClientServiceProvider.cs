using Microsoft.Teams.AI;
using TeamsAIssistant.Config;
using TeamsAIssistant.Repositories;

namespace TeamsAIssistant.Services;

public class SimplicateClientServiceProvider(TeamsAdapter teamsAdapter, KeyVaultRepository keyVaultRepository, IConfiguration configuration)
{
    private readonly string? _vaultName = configuration.Get<ConfigOptions>()!.SimplicateVaultName;
    private readonly string? _environment = configuration.Get<ConfigOptions>()!.SimplicateVaultName?.Split("-").FirstOrDefault();
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

        var credentials = await GetCredentials(aadObjectId);

        _httpClient = teamsAdapter.HttpClientFactory.CreateClient("SimplicateClient");
        _httpClient.BaseAddress = new Uri($"https://{_environment}.simplicate.nl/api/v2/");

        _httpClient.DefaultRequestHeaders.Add("Authentication-Key", credentials.Key);
        _httpClient.DefaultRequestHeaders.Add("Authentication-Secret", credentials.Secret);

        return _httpClient;
    }

    public async Task<(string? Environment, string Key, string Secret)> GetCredentials(string aadObjectId)
    {
        if (_environment == null || _vaultName == null)
        {
            throw new ArgumentException("Simplicate configuration missing");
        }

        var secret = await keyVaultRepository.GetSecret(_vaultName, aadObjectId);

        return (Environment: _environment, secret.Properties.ContentType, secret.Value);

    }
}
