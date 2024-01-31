using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Repositories;

public class WebRepository
{
  private readonly HttpClient _httpClient;

  public WebRepository(IHttpClientFactory httpClientFactory)
  {
    _httpClient = httpClientFactory.CreateClient("DownloadClient");
    _httpClient.DefaultRequestHeaders.Add("User-Agent", AIConstants.AIUserAgent);
  }

  public async Task<byte[]?> DownloadFileAsync(string url)
  {
    var response = await _httpClient.GetAsync(url);

    if (response.IsSuccessStatusCode)
    {
      var contentStream = await response.Content.ReadAsByteArrayAsync();

      return contentStream;
    }
    else
    {
      throw new HttpRequestException(response.ReasonPhrase);
    }
  }
}
