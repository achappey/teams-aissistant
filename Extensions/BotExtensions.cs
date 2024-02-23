using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace TeamsAIssistant.Extensions
{
    public static class BotExtensions
    {
        public static string GetActionSubmitText(this ITurnContext context, string actionName, string result)
        {
            return $"{context.Activity.From.Name} submitted {actionName} action. Result: {result}";
        }

        public static IEnumerable<Models.File>? ToFiles(this IEnumerable<Attachment> attachments)
        {
            var inlineFiles = attachments.Where(t => string.IsNullOrEmpty(t.ContentUrl)).SelectMany(ExtractFiles) ?? [];

            return inlineFiles.Concat(attachments.Where(t => !string.IsNullOrEmpty(t.ContentUrl)).Select(u => new Models.File()
            {
                Filename = !string.IsNullOrEmpty(u.Name) ? u.Name : u.ContentUrl.FindNameFromUrl(),
                Url = u.ContentUrl,
            }))
            .GroupBy(file => file.Filename)
            .Select(group => group.First());
        }

        public static IEnumerable<Models.File> ExtractFiles(this Attachment attachment)
        {
            var resources = new List<Models.File>();

            switch (attachment.ContentType)
            {
                case "text/html":
                    var htmlContent = attachment.Content?.ToString();
                    var hrefs = htmlContent?.ExtractAllHrefs();

                    if (hrefs != null)
                    {
                        foreach (var href in hrefs)
                        {
                            if (href.StartsWith("http"))
                            {
                                resources.Add(new() { Url = href, Filename = href.FindNameFromUrl() });
                            }
                        }
                    }

                    break;
                default:
                    break;
            }

            return resources;
        }

    }
}