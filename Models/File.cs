using System.Text.Json.Serialization;

namespace TeamsAIssistant.Models;

public class File
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("bytes")]
    public int? Bytes { get; set; }

    [JsonPropertyName("content")]
    public byte[]? Content { get; set; }

    public string CreatedAtString
    {
        get
        {
            return CreatedAt.ToString();
        }
    }
}