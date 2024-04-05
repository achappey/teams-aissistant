using OpenAI;
using OpenAI.Managers;
using System.Reflection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Teams.AI;
using Microsoft.Teams.AI.AI;
using Microsoft.Teams.AI.AI.Planners.Experimental;
using Microsoft.Teams.AI.AI.Planners;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Teams.AI.AI.Models;
using Microsoft.Teams.AI.AI.Prompts;
using Microsoft.KernelMemory.DataFormats.WebPages;
using TeamsAIssistant.Handlers;
using TeamsAIssistant.Config;
using TeamsAIssistant.Services;
using TeamsAIssistant;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.State;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Handlers.Plugins;
using TeamsAIssistant.Planner;
using TeamsAIssistant.DataSources;
using MailChimp.Net;
using Newtonsoft.Json;

////ngrok http 5130 --host-header="localhost:5130"
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Load configuration
var config = builder.Configuration.Get<ConfigOptions>()!;
if (config.OpenAI == null || string.IsNullOrEmpty(config.OpenAI.ApiKey))
{
    throw new Exception("Missing OpenAI configuration.");
}

// Missing Assistant ID, create new Assistant
if (string.IsNullOrEmpty(config.OpenAI.AssistantId))
{
    throw new Exception("Missing OpenAI Assistant.");
}

// Prepare Configuration for ConfigurationBotFrameworkAuthentication
builder.Configuration["MicrosoftAppType"] = "MultiTenant";
builder.Configuration["MicrosoftAppId"] = config.BOT_ID;
builder.Configuration["MicrosoftAppPassword"] = config.BOT_PASSWORD;

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Cloud Adapter with error handling enabled.
// Note: some classes expect a BotAdapter and some expect a BotFrameworkHttpAdapter, so
// register the same adapter instance for all types.
builder.Services.AddSingleton<TeamsAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetService<TeamsAdapter>()!);

builder.Services.AddSingleton<WebRepository>();
builder.Services.AddSingleton<AssistantRepository>();
builder.Services.AddSingleton<FileRepository>();
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<ProactiveMessageService>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<WebScraper>();
builder.Services.AddScoped<AssistantHandlers>();
builder.Services.AddScoped<AssistantService>();
builder.Services.AddScoped<IndexService>();
builder.Services.AddScoped<PluginService>();
builder.Services.AddScoped<ConversationHandlers>();
builder.Services.AddScoped<FileHandlers>();
builder.Services.AddScoped<KernelMemoryData>();

builder.Services.AddSingleton<OpenAIModel>(sp => new(
    new OpenAIModelOptions(config.OpenAI.ApiKey, OpenAI.ObjectModels.Models.Gpt_4_0125_preview),
    sp.GetService<ILoggerFactory>()
));

builder.Services.AddScoped<GraphClientServiceProvider>();
builder.Services.AddScoped<SimplicateClientServiceProvider>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

var allTypes = Assembly.GetExecutingAssembly().GetTypes();
var pluginTypes = allTypes.Where(t => typeof(PluginBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

foreach (var type in pluginTypes)
{
    builder.Services.AddScoped(type);
}

builder.Services.AddSingleton(sp => new MailChimpManager(new MailChimpOptions()
{
    ApiKey = config.Mailchimp?.ApiKey,
    DataCenter = config.Mailchimp?.DataCenter,
}));

builder.Services.AddSingleton(sp => new OpenAIClient(new OpenAIAuthentication(config.OpenAI.ApiKey, config.OpenAI.Organization)));
builder.Services.AddSingleton(sp => new OpenAIService(new OpenAiOptions()
{
    ApiKey = config.OpenAI.ApiKey,
    Organization = config.OpenAI.Organization,
    DefaultModelId = OpenAI.ObjectModels.Models.Gpt_4_0125_preview
}));

if (!string.IsNullOrEmpty(config.AzureBlobStorageConnectionString) && !string.IsNullOrEmpty(config.AzureBlobContainerName))
{
    builder.Services.AddSingleton<IStorage>(new AzureStorageRepository(config.AzureBlobStorageConnectionString, config.AzureBlobContainerName));
}
else
{
    builder.Services.AddSingleton<IStorage, MemoryStorage>();
}

builder.Services.AddSingleton(_ => new AssistantsPlannerOptions(config.OpenAI.ApiKey, config.OpenAI.AssistantId)
{
    PollingInterval = TimeSpan.FromMilliseconds(500),
    Organization = config.OpenAI.Organization,
});

builder.Services.AddSingleton<KeyVaultClientProvider>();
builder.Services.AddSingleton<KeyVaultRepository>();
builder.Services.AddScoped<DriveRepository>();
builder.Services.AddScoped<DownloadService>();
builder.Services.AddScoped<AttachmentHandlers>();
builder.Services.AddScoped<ExtensionsHandlers>();
builder.Services.AddScoped<ActionHandlers>();
builder.Services.AddScoped<ConversationFilesService>();

builder.Services.AddSingleton(sp =>
{
    IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(config.AAD_APP_CLIENT_ID)
                                        .WithClientSecret(config.AAD_APP_CLIENT_SECRET)
                                        .WithTenantId(config.AAD_APP_TENANT_ID)
                                        .WithLegacyCacheCompatibility(false)
                                        .Build();
    app.AddInMemoryTokenCache();

    return app;
});

builder.Services.AddSingleton<KeyVaultClientProvider>();

// Create the Application.
builder.Services.AddTransient<IBot>(sp =>
{
    var msal = sp.GetRequiredService<IConfidentialClientApplication>();
    var adapter = sp.GetRequiredService<TeamsAdapter>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var graphClientServiceProvider = sp.GetRequiredService<GraphClientServiceProvider>();
    var assistantService = sp.GetRequiredService<AssistantService>();
    var indexService = sp.GetService<IndexService>();

    PromptManager prompts = new(new()
    {
        PromptFolder = "./Prompts",
        MaxConversationHistoryTokens = 1024
    });

    ActionPlannerOptions<TeamsAIssistantState> actionPlannerOptions = new(
            model: sp.GetService<OpenAIModel>()!,
            prompts: prompts,
            defaultPrompt: async (context, state, planner) =>
            {
                PromptTemplate template = prompts.GetPrompt("Chat");

                return await Task.FromResult(template);
            }
        );

    IPlanner<TeamsAIssistantState> planner = new TeamsAIssistantPlanner(
        actionPlannerOptions,
        sp.GetRequiredService<KernelMemoryData>(),
        adapter,
        assistantService,
        sp.GetRequiredService<AssistantsPlannerOptions>(),
        loggerFactory);

    // Start building the application
    var appBuilder = new ApplicationBuilder<TeamsAIssistantState>()
           .WithStorage(sp.GetRequiredService<IStorage>())
           .WithAIOptions(new AIOptions<TeamsAIssistantState>(planner))
           .WithLoggerFactory(loggerFactory)
           .WithLongRunningMessages(adapter, config.BOT_ID!);

    AuthenticationOptions<TeamsAIssistantState> options = new();
    var dataverseConnections = config.DataverseConnections?.ToStringList() ?? [];

    if (!string.IsNullOrEmpty(config.CONNECTION_NAME))
    {
        options.AddAuthentication(Auth.Graph, new OAuthSettings()
        {
            ConnectionName = config.CONNECTION_NAME,
            Text = $"Login at {config.CONNECTION_NAME}",
            EnableSso = true,
            OAuthAppCredentials = new MicrosoftAppCredentials(config.AAD_APP_CLIENT_ID,
                config.AAD_APP_CLIENT_SECRET, oAuthScope: config.AAD_APP_SCOPES!, channelAuthTenant: config.AAD_APP_TENANT_ID),
            Title = "Click here"
        });

        foreach (var connection in dataverseConnections)
        {
            var connectionName = connection.Split(";").ElementAt(0);
            var resourceName = connection.Split(";").ElementAt(1);
            string resource = $"https://{resourceName}.dynamics.com";
            var scope = resource + "/.default";

            options.AddAuthentication(connectionName, new OAuthSettings()
            {
                ConnectionName = connectionName,
                Text = $"Login at {connectionName}",
                EnableSso = true,
                OAuthAppCredentials = new MicrosoftAppCredentials(config.AAD_APP_CLIENT_ID,
                              config.AAD_APP_CLIENT_SECRET, oAuthScope: scope, channelAuthTenant: config.AAD_APP_TENANT_ID),
                Title = "Click here"
            });
        }



    }
    else
    {
        string signInLink = $"https://{config.BOT_DOMAIN}/auth-start.html";

        options.AddAuthentication(Auth.Graph, new TeamsSsoSettings(config.AAD_APP_SCOPES!.Split(","), signInLink, msal));
/*
        foreach (var connection in dataverseConnections)
        {
            var connectionName = connection.Split(";").ElementAt(0);
            var resourceName = connection.Split(";").ElementAt(1);
            string resource = $"https://{resourceName}.dynamics.com";
            var scope = resource + "/.default";
            options.AddAuthentication(connection, new OAuthSettings()
            {
                ConnectionName = connectionName,
                Text = $"Login at {connectionName}",
                EnableSso = true,
                OAuthAppCredentials = new MicrosoftAppCredentials(config.AAD_APP_CLIENT_ID,
                              config.AAD_APP_CLIENT_SECRET, oAuthScope: scope, channelAuthTenant: config.AAD_APP_TENANT_ID),
                Title = "Click here"
            });
        }*/
    }

    appBuilder = appBuilder.WithAuthentication(adapter, options);

    // Complete the application building
    Application<TeamsAIssistantState> app = appBuilder.Build();

    // Register default AI actions
    var actionHandlers = sp.GetRequiredService<ActionHandlers>();
    app.AI.ImportActions(actionHandlers);

    // Register AI plugin actions
    var pluginTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginBase)))
    .ToList();

    foreach (var type in pluginTypes)
    {
        var plugin = (PluginBase?)sp.GetService(type);
        if (plugin != null)
        {
            app.AI.ImportActions(plugin);

            var pluginItem = plugin.GetPlugin();

            if (pluginItem.Submits != null)
            {
                foreach (var (name, handler) in pluginItem.Submits)
                {
                    app.AdaptiveCards.OnActionSubmit(name, async (ITurnContext turnContext, TeamsAIssistantState turnState, object data, CancellationToken cancellationToken) =>
                    {
                        await handler(turnContext, turnState, data, cancellationToken);

                        await app.AI.RunAsync(turnContext, turnState);
                    });
                }
            }
        }
    }

    var attachmentHandlers = sp.GetRequiredService<AttachmentHandlers>();
    var assistantHandlers = sp.GetRequiredService<AssistantHandlers>();
    var extensionsHandlers = sp.GetRequiredService<ExtensionsHandlers>();
    var fileHandlers = sp.GetRequiredService<FileHandlers>();
    var conversationHandlers = sp.GetRequiredService<ConversationHandlers>();
    var memCache = sp.GetRequiredService<IMemoryCache>();

    app.OnMessage("/export", attachmentHandlers.ExportMessagesHandler);
    app.OnMessage("/reset", conversationHandlers.HandleResetMessageHandler);

    app.OnMessage("/files", fileHandlers.SourcesMessageHandler);
    app.OnMessage("/assistant", assistantHandlers.AssistantMessageHandler);
    app.OnMessage("/extensions", extensionsHandlers.MenuHandler);
    app.OnMessage("/menu", conversationHandlers.MenuHandler);
    //app.OnConversationUpdate(ConversationUpdateEvents.MembersAdded, conversationHandlers.MemberAddedHandler);



    app.OnBeforeTurn(async (turnContext, turnState, cancellationToken) =>
    {
        return await Task.Run(async () =>
        {
            conversationHandlers.EnsureDefaultSources(turnState, config);
            var token = turnState.GetGraphToken();
            graphClientServiceProvider.SetToken(token);

            if (turnState.User?.ContainsKey("__InSignInFlow__") == true)
            {
                return true;
            }

            foreach (var connection in dataverseConnections)
            {
                var connectionName = connection.Split(";").ElementAt(0);
                var cacheKey = $"AuthTokens_{connectionName}_{graphClientServiceProvider.AadObjectId}";

                var cachedToken = memCache.Get<string>(cacheKey);

                if (string.IsNullOrEmpty(cachedToken))
                {
                    var dataverse = await app.GetTokenOrStartSignInAsync(turnContext, turnState, connectionName, cancellationToken);

                    if (dataverse != null)
                    {
                        graphClientServiceProvider.SetDataverseToken(connectionName, dataverse);
                        memCache.Set(cacheKey, dataverse, TimeSpan.FromHours(1));
                    }
                    else
                    {
                        return false;
                    }
                }
                else {
                     graphClientServiceProvider.SetDataverseToken(connectionName, cachedToken);
                }

            }

            return true;
        });
    });

    app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteFileVerb, attachmentHandlers.DeleteFileHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteAssistantFileVerb, attachmentHandlers.DeleteAssistantFileHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.FileToAssistantVerb, attachmentHandlers.AttachFileHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.AddToChatVerb, attachmentHandlers.AddToChatFileHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.ExportVerb, attachmentHandlers.ExportButtonHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.CloneAssistantVerb, assistantHandlers.CloneAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteAssistantVerb, assistantHandlers.DeleteAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdatePluginsVerb, extensionsHandlers.UpdatePluginsHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateKernelMemoryVerb, extensionsHandlers.UpdateKernelMemoryHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.ClearConversationVerb, conversationHandlers.ResetConversationHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.FilesVerb, fileHandlers.ShowFilesHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.AssistantVerb, assistantHandlers.ShowAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.ExtensionsVerb, extensionsHandlers.ShowExtensionsHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateAssistantVerb, assistantHandlers.UpdateAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateConversationVerb, conversationHandlers.UpdateConversationHandler);

    app.OnActivity(ActivityTypes.Message, async (turnContext, turnState, cancellationToken) =>
    {
        await HandleActivityAsync(turnContext, turnState, cancellationToken, graphClientServiceProvider.AadObjectId!, app, sp, attachmentHandlers, conversationHandlers);
    });

    app.Authentication.Get(Auth.Graph).OnUserSignInSuccess(async (context, state) =>
    {
        await HandleActivityAsync(context, state, CancellationToken.None, graphClientServiceProvider.AadObjectId!, app, sp, attachmentHandlers, conversationHandlers);
    });

    app.Authentication.Get(Auth.Graph).OnUserSignInFailure(async (context, state, ex) =>
    {
        await context.SendActivityAsync("Failed to login");
    });


    return app;
});

static async Task HandleActivityAsync(ITurnContext turnContext,
    TeamsAIssistantState turnState, CancellationToken cancellationToken, string userId,
    Application<TeamsAIssistantState> app, IServiceProvider sp, AttachmentHandlers attachmentHandlers, ConversationHandlers conversationHandlers)
{
    if (turnContext.Activity.Text != null && turnContext.Activity.Text.Trim().Equals($"/{turnContext.Activity.Recipient.Name}"))
    {
        await conversationHandlers.HandleMenuAsync(turnContext, turnState, cancellationToken);
    }
    else if (turnContext.Activity.Text != null && turnContext.Activity.Text.IsAuthCode())
    {
        await conversationHandlers.HandleMenuAsync(turnContext, turnState, cancellationToken);
    }
    else
    {

        await attachmentHandlers.HandleAttachmentsAsync(turnContext, turnState, cancellationToken);

        turnState.Temp.AdditionalInstructions += turnContext.GetAdditionalInstructions(turnState, userId);

        /* TeamsChannelData channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

         if (channelData?.Team != null)
         {
             if (turnState.SiteIndexes == null || !turnState.SiteIndexes.Contains(channelData.Team.Id))
             {
                 turnState.SiteIndexes = turnState.SiteIndexes?.EnsureId(channelData.Team.Id);
             }
         }*/

        await turnState.SaveStateAsync(turnContext, sp.GetService<IStorage>());
        await app.AI.RunAsync(turnContext, turnState);

        if (turnState.ExportToolCalls.HasValue && turnState.ExportToolCalls.Value)
        {
            var assistantService = sp.GetRequiredService<AssistantService>();
            var conversationFilesService = sp.GetService<ConversationFilesService>();

            if (conversationFilesService != null && turnState.ThreadId != null && turnState.RunId != null)
            {
                var toolCalls = await assistantService.GetToolCalls(turnState.ThreadId, turnState.RunId);
                var items = toolCalls.Select(n => new { n.input, logs = string.Join(",", n.logs) });

                if (items.Any())
                {
                    var csvFile = JsonConvert.SerializeObject(items).ConvertJsonToCsv();

                    if (csvFile != null)
                    {
                        await conversationFilesService.SaveFile(turnContext, new()
                        {
                            Filename = $"ToolCalls-{DateTime.Now.Ticks}.csv",
                            Content = csvFile
                        });
                    }
                }
            }
        }
    }
}


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
