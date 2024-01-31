using Azure.Security.KeyVault.Secrets;
using TeamsAIssistant.Models;
using TeamsAIssistant.Services;

namespace TeamsAIssistant.Repositories;

public class KeyVaultRepository(KeyVaultClientProvider keyVaultClientProvider)
{
  private readonly KeyVaultClientProvider _keyVaultClientProvider = keyVaultClientProvider;

    public async Task<KeyVaultSecret> GetSecret(string vault, string name)
  {
    var client = await GetSecretClientAsync(vault);
    var secret = await client.GetSecretAsync(name);
    return secret.Value;
  }

  private async Task<SecretClient> GetSecretClientAsync(string vault)
  {
    var accessToken = await _keyVaultClientProvider.GetAccessTokenAsync();
    var credential = new AccessTokenCredential(accessToken);

    var kvUri = $"https://{vault}.vault.azure.net";

    return new SecretClient(new Uri(kvUri), credential);
  }

}
