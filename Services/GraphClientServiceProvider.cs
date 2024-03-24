using Microsoft.Graph.Beta;
using Microsoft.Teams.AI;
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Services;

public class GraphClientServiceProvider(TeamsAdapter teamsAdapter)
{

    private IDictionary<string, string> _dataverseTokens = new Dictionary<string, string>();
    private string? _token;
    private string? _aadObjectId;

    private GraphServiceClient? _graphServiceClient;

    public void SetDataverseToken(string key, string token)
    {
        if (_dataverseTokens.ContainsKey(key))
        {
            _dataverseTokens[key] = token;
        }
        else
        {
            _dataverseTokens.Add(key, token);
        }
    }

    public string? GetDataverseToken(string key)
    {
        return _dataverseTokens[key];
    }

    public void SetToken(string? token)
    {
        _token = token;
        _aadObjectId = token?.DecodeAccessToken();
    }

    public string? GetToken()
    {
        return _token;
    }

    public string? AadObjectId
    {
        get
        {
            return _aadObjectId;
        }
    }

    public bool IsAuthenticated()
    {
        return _token != null;
    }

    public GraphServiceClient GetAuthenticatedGraphClient()
    {
        if (_token == null)
        {
            throw new UnauthorizedAccessException();
        }

        if (_graphServiceClient != null)
        {
            return _graphServiceClient;
        }

        var httpClient = teamsAdapter.HttpClientFactory.CreateClient("AuthenticatedWebClient");
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _token);
        httpClient.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");
        httpClient.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"W. Europe Standard Time\"");
        httpClient.DefaultRequestHeaders.Add("Prefer", "outlook.body-content-type=\"text\"");

        _graphServiceClient = new(httpClient);
        return _graphServiceClient;
    }
}
