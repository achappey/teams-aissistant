using System.Text.Json.Serialization;

namespace TeamsAIssistant.Models
{
    public class SimplicateItemData
    {
        public SimplicateItem? Data { get; set; }
    }


    public class SimplicateItem
    {
        public string? Id { get; set; }

        [JsonPropertyName("simplicate_url")]
        public string? SimplicateUrl { get; set; }
    }
}
