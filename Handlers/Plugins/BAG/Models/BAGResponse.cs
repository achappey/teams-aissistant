using System.Text.Json.Serialization;

namespace TeamsAIssistant.Handlers.Plugins.BAG.Models;

internal class BAGResponse
{
    [JsonPropertyName("_embedded")]
    public Embedded? Embedded { get; set; }

    [JsonPropertyName("_links")]
    public PagingLinks? Links { get; set; }
}

internal class Embedded
{
    public List<Adres>? Adressen { get; set; }
}

internal class PagingLinks
{
    public Self? Self { get; set; }
    public Self? Next { get; set; }
    public Self? Last { get; set; }

}

internal class Self
{
    public string? Href { get; set; }
}

internal class Building
{
    public string? Href { get; set; }
}