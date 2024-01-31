
namespace TeamsAIssistant.Handlers.Plugins.Mailchimp.Models;

public class MailchimpQuery
{
    public int Offset { get; set; }
    public int Count { get; set; }
}

public static class CampaignTypeConstants
{
    public const string Regular = "regular";
    public const string Plaintext = "plaintext";
    public const string Absplit = "absplit";
    public const string Rss = "rss";
    public const string Variate = "variate";
}

public static class CampaignStatusConstants
{
    public const string Save = "save";
    public const string Paused = "paused";
    public const string Schedule = "schedule";
    public const string Sending = "sending";
    public const string Sent = "sent";
}