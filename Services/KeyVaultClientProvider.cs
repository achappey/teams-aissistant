using Microsoft.Identity.Client;

namespace TeamsAIssistant.Services;

public class KeyVaultClientProvider(IConfidentialClientApplication confidentialClientApplication)
{
    private string? _cachedToken;
    private DateTimeOffset _expiryTime;

    public async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _expiryTime)
        {
            return _cachedToken;
        }

        var result = await confidentialClientApplication.AcquireTokenForClient(["https://vault.azure.net/.default"]).ExecuteAsync();
        _cachedToken = result.AccessToken;
        _expiryTime = result.ExpiresOn;

        return _cachedToken;
    }
}
