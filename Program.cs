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
using System.Reflection;
using OpenAI;
using OpenAI.Managers;
using TeamsAIssistant.Handlers;
using TeamsAIssistant.Config;
using TeamsAIssistant.Services;
using TeamsAIssistant;
using TeamsAIssistant.Constants;
using TeamsAIssistant.Repositories;
using TeamsAIssistant.State;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Handlers.Plugins;
using MailChimp.Net;
using Newtonsoft.Json;
using Microsoft.Teams.AI.AI.Models;
using Microsoft.Teams.AI.AI.Prompts;
using TeamsAIssistant.Planner;
using TeamsAIssistant.DataSources;
using Microsoft.KernelMemory.DataFormats.WebPages;

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
       new OpenAIModelOptions(config.OpenAI.ApiKey, OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_1106),
       sp.GetService<ILoggerFactory>()
));

if (!string.IsNullOrEmpty(config.AAD_APP_CLIENT_ID))
{
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


if (!string.IsNullOrEmpty(config.AAD_APP_CLIENT_ID))
{
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
}

// Create the Application.
builder.Services.AddTransient<IBot>(sp =>
{
    IConfidentialClientApplication? msal = sp.GetService<IConfidentialClientApplication>();
    AuthenticationOptions<TeamsAIssistantState> options = new();
    var adapter = sp.GetRequiredService<TeamsAdapter>();
    ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var graphClientServiceProvider = sp.GetService<GraphClientServiceProvider>();
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
        assistantService,
        sp.GetRequiredService<AssistantsPlannerOptions>(),
        loggerFactory);

    // Start building the application
    var appBuilder = new ApplicationBuilder<TeamsAIssistantState>()
           .WithStorage(sp.GetRequiredService<IStorage>())
           .WithAIOptions(new AIOptions<TeamsAIssistantState>(planner))
           .WithLoggerFactory(loggerFactory);

    // Conditionally add authentication
    if (msal != null)
    {
        string signInLink = $"https://{config.BOT_DOMAIN}/auth-start.html";
        options.AddAuthentication(Auth.Graph, new TeamsSsoSettings(config.AAD_APP_SCOPES!.Split(","), signInLink, msal));
        appBuilder = appBuilder.WithAuthentication(adapter, options);
    }

    // Complete the application building
    Application<TeamsAIssistantState> app = appBuilder.Build();

    // Register default AI actions
    var actionHandlers = sp.GetService<ActionHandlers>();
    if (actionHandlers != null)
    {
        app.AI.ImportActions(actionHandlers);
    }

    // Register AI plugin actions
    if (config.AAD_APP_CLIENT_ID != null)
    {
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
    }

    var attachmentHandlers = sp.GetService<AttachmentHandlers>();
    var assistantHandlers = sp.GetRequiredService<AssistantHandlers>();
    var extensionsHandlers = sp.GetRequiredService<ExtensionsHandlers>();
    var fileHandlers = sp.GetRequiredService<FileHandlers>();
    var conversationHandlers = sp.GetRequiredService<ConversationHandlers>();

    if (attachmentHandlers != null)
    {
        app.OnMessage("/export", attachmentHandlers.ExportMessagesHandler);
    }

    app.OnMessage("/reset", conversationHandlers.HandleResetMessageHandler);
    app.OnMessage("/files", fileHandlers.SourcesMessageHandler);
    app.OnMessage("/assistant", assistantHandlers.AssistantMessageHandler);
    app.OnMessage("/extensions", extensionsHandlers.MenuHandler);
    app.OnMessage("/menu", conversationHandlers.MenuHandler);

    if (graphClientServiceProvider != null)
    {
        app.OnBeforeTurn(async (turnContext, turnState, cancellationToken) =>
        {
            return await Task.Run(() =>
            {
                var token = turnState.GetGraphToken();
                graphClientServiceProvider.SetToken(token);

                return true;
            });
        });
    }

    if (attachmentHandlers != null)
    {
        app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteFileVerb, attachmentHandlers.DeleteFileHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteAssistantFileVerb, attachmentHandlers.DeleteAssistantFileHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.FileToAssistantVerb, attachmentHandlers.AttachFileHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.AddToChatVerb, attachmentHandlers.AddToChatFileHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.ExportVerb, attachmentHandlers.ExportButtonHandler);
    }

    if (!string.IsNullOrEmpty(config.AAD_APP_CLIENT_ID))
    {
        app.AdaptiveCards.OnActionSubmit(SubmitActions.CloneAssistantVerb, assistantHandlers.CloneAssistantHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.DeleteAssistantVerb, assistantHandlers.DeleteAssistantHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdatePluginsVerb, extensionsHandlers.UpdatePluginsHandler);
        app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateKernelMemoryVerb, extensionsHandlers.UpdateKernelMemoryHandler);
    }

    app.AdaptiveCards.OnActionSubmit(SubmitActions.ClearConversationVerb, conversationHandlers.ResetConversationHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.FilesVerb, fileHandlers.ShowFilesHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.AssistantVerb, assistantHandlers.ShowAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.ExtensionsVerb, extensionsHandlers.ShowExtensionsHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateAssistantVerb, assistantHandlers.UpdateAssistantHandler);
    app.AdaptiveCards.OnActionSubmit(SubmitActions.UpdateConversationVerb, conversationHandlers.UpdateConversationHandler);

    app.OnActivity(ActivityTypes.Message, async (turnContext, turnState, cancellationToken) =>
    {
        await HandleActivityAsync(turnContext, turnState, CancellationToken.None, app, sp, attachmentHandlers, conversationHandlers);
    });

    if (!string.IsNullOrEmpty(config.AAD_APP_CLIENT_ID))
    {
        app.Authentication.Get("graph").OnUserSignInSuccess(async (context, state) =>
        {
            await HandleActivityAsync(context, state, CancellationToken.None, app, sp, attachmentHandlers, conversationHandlers);
        });

        app.Authentication.Get("graph").OnUserSignInFailure(async (context, state, ex) =>
        {
            await context.SendActivityAsync("Failed to login");
            await context.SendActivityAsync($"Error message: {ex.Message}");
        });
    }

    return app;
});

static async Task HandleActivityAsync(ITurnContext turnContext, TeamsAIssistantState turnState, CancellationToken cancellationToken,
    Application<TeamsAIssistantState> app, IServiceProvider sp, AttachmentHandlers? attachmentHandlers, ConversationHandlers conversationHandlers)
{
    if (turnContext.Activity.Text != null && turnContext.Activity.Text.Trim().Equals($"/{turnContext.Activity.Recipient.Name}"))
    {
        await conversationHandlers.HandleMenuAsync(turnContext, turnState, cancellationToken);
    }
    else
    {
        if (attachmentHandlers != null)
        {
            await attachmentHandlers.HandleAttachmentsAsync(turnContext, turnState, cancellationToken);
        }

        turnState.Temp.AdditionalInstructions += turnContext.GetAdditionalInstructions(turnState);

        /* TeamsChannelData channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

         if (channelData?.Team != null)
         {
             if (turnState.SiteIndexes == null || !turnState.SiteIndexes.Contains(channelData.Team.Id))
             {
                 turnState.SiteIndexes = turnState.SiteIndexes?.EnsureId(channelData.Team.Id);
             }
         }*/

        await turnState.SaveStateAsync(turnContext, sp.GetService<IStorage>());
        await app.AI.RunAsync(turnContext, turnState, cancellationToken: cancellationToken);

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
