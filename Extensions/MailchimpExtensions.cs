using TeamsAIssistant.Handlers.Plugins.Mailchimp.Models;

namespace TeamsAIssistant.Extensions
{
    public static class MailchimpExtensions
    {

        public static MailchimpQuery ToMailchimpQuery(this Dictionary<string, object> parameters)
        {
            return new () {
                  Offset = parameters.TryGetValue("offset", out object? value) ? Convert.ToInt32(value?.ToString()) : 0,
                  Count = parameters.TryGetValue("count", out object? count) ? Convert.ToInt32(count?.ToString()) : 0,
            };
        }
    }
}