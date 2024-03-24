namespace TeamsAIssistant.Config
{
    public class ConfigOptions
    {
        public string? BOT_ID { get; set; }
        public string? BOT_PASSWORD { get; set; }
        public string? BOT_DOMAIN { get; set; }
        public string? CONNECTION_NAME { get; set; }
        public string? AAD_APP_CLIENT_ID { get; set; }
        public string? AAD_APP_CLIENT_SECRET { get; set; }
        public string? AAD_APP_TENANT_ID { get; set; }
        public string? AAD_APP_OAUTH_AUTHORITY_HOST { get; set; }
        public string? AAD_APP_SCOPES { get; set; }
        public OpenAIConfigOptions? OpenAI { get; set; }
        public string? AzureBlobContainerName { get; set; }
        public string? AzureBlobStorageConnectionString { get; set; }
        public string? SimplicateVaultName { get; set; }
        public Mailchimp? Mailchimp { get; set; }
        public string? BAGApiKey { get; set; }
        public string? AzureMapsSubscriptionKey { get; set; }        
        public string? IndexQueue { get; set; }        
        public string? SearchEndpoint { get; set; }
        public string? TypeFilters { get; set; }
        public string? DriveIndexes { get; set; }
        public string? SiteIndexes { get; set; }
        public double? MinRelevance { get; set; }
        public string? DataverseConnections { get; set; }
        public string? Pexels { get; set; }
                
    }

    public class Mailchimp
    {
        public string? ApiKey { get; set; }
        public string? DataCenter { get; set; }
    }

    /// <summary>
    /// Options for Open AI
    /// </summary>
    public class OpenAIConfigOptions
    {
        public string? ApiKey { get; set; }
        public string? AssistantId { get; set; }
        public string? Organization { get; set; }
    }
}
