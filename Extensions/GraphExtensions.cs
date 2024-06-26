
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Extensions
{
    public static class GraphExtensions
    {

        private static async Task AddFilesRecursivelyAsync(DriveItem folder, GraphServiceClient graphClient, List<DriveItem> allFiles)
        {
            var children = await graphClient.Drives[folder?.ParentReference?.DriveId].Items[folder?.Id].Children
                .GetAsync();

            foreach (var item in children?.Value ?? [])
            {
                if (item.Folder != null)
                {
                    await AddFilesRecursivelyAsync(item, graphClient, allFiles);
                }
                else
                {
                    allFiles.Add(item);
                }
            }
        }


        public static Recipient ToRecipient(this string address)
        {
            return new()
            {
                EmailAddress = address.ToEmailAddress()
            };
        }

        public static EmailAddress ToEmailAddress(this string address)
        {
            return new()
            {
                Address = address
            };
        }

        public static ChatMessage ToChatMessage(this JObject jObject)
        {
            return new()
            {
                Body = new()
                {
                    ContentType = Enum.Parse<BodyType>(jObject?["contentType"]?.ToString() ??
                                Enum.GetName(BodyType.Text)!),
                    Content = jObject?["content"]?.ToString(),
                },
            };
        }


        public static DateTimeTimeZone ToTimeZone(this string item)
        {
            return new()
            {
                DateTime = item
            };
        }

        public static string ToGraphUserSearchString(this Dictionary<string, object> parameters)
        {
            return parameters.Where(e => e.Key == "displayName" || e.Key == "topic")
                                        .ToDictionary(a => a.Key, a => a.Value).ToGraphSearchString();
        }

        public static string? ToGraphUserFilterString(this Dictionary<string, object> parameters)
        {
            return parameters.Where(e => e.Key != "displayName" && e.Key != "topic")
                                        .ToDictionary(a => a.Key, a => a.Value).ToGraphFilterString();
        }

        public static string ToGraphSearchString(this Dictionary<string, object> parameters)
        {
            return string.Join(" AND ", parameters
                .Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                .Select(kv => $"\"{kv.Key}:{kv.Value?.ToString()?.Replace(" ", "")}\""));
        }

        public static string? ToGraphFilterString(this Dictionary<string, object> parameters)
        {
            var result = string.Join(" and ", parameters
                .Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                .Select(kv => $"{kv.Key} eq '{Uri.EscapeDataString(kv.Value?.ToString()!)}'"));

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return null;
        }

        public static Task<DriveItem?> GetDriveItem(this GraphServiceClient client, string link)
        {
            string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(link));
            string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');

            return client.Shares[encodedUrl].DriveItem.GetAsync();
        }

        public static async Task<byte[]> GetDriveItemContent(this GraphServiceClient client, string driveId, string itemId)
        {
            await using var stream = await client.Drives[driveId].Items[itemId].Content
                .GetAsync();

            if (stream == null)
            {
                throw new InvalidOperationException("Stream cannot be null");
            }

            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }

    }
}