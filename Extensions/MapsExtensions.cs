namespace TeamsAIssistant.Extensions
{
    public static class MapsExtensions
    {
        public static (string latitude, string longitude) GetLatLong(this Dictionary<string, object> parameters)
        {
            return (parameters["latitude"].ToString()!, parameters["longitude"].ToString()!);
        }

        public static string ToMapsFilterString(this Dictionary<string, object> parameters, string[] excluded)
        {
            return string.Join("&", parameters
                .Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                .Where(y => !excluded.Contains(y.Key))
                .Select(t => $"{t.Key}={t.Value}"));
        }
    }
}