using Azure.Core;

namespace TeamsAIssistant.Models;

public class AccessTokenCredential(string accessToken) : TokenCredential
{
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new(accessToken, DateTimeOffset.UtcNow.AddHours(1));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new(GetToken(requestContext, cancellationToken));
    }
}
