using Microsoft.Teams.AI.AI.Action;
using System.ComponentModel;
using TeamsAIssistant.Models;
using Microsoft.Teams.AI.AI.OpenAI.Models;
using System.Reflection;
using TeamsAIssistant.Repositories;
using Microsoft.Bot.Builder;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.State;
using TeamsAIssistant.Services;
using Microsoft.Teams.AI;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.AdaptiveCards;
using System.Globalization;

namespace TeamsAIssistant.Handlers.Plugins
{
    public abstract class PluginBase(
            DriveRepository driveRepository,
            ProactiveMessageService proactiveMessageService,
            string pluginName,
            string publisher,
            string apiName,
            string apiVersion)

    {
        public string PluginName { get; private set; } = pluginName;
        public string Publisher { get; private set; } = publisher;
        public string ApiName { get; private set; } = apiName;
        public string ApiVersion { get; private set; } = apiVersion;

        public string SourceName
        {
            get
            {
                return $"{Publisher} {ApiName} {ApiVersion}";
            }
        }


        public Plugin GetPlugin()
        {
            var plugin = new Plugin
            {
                DisplayName = PluginName,
                Publisher = Publisher,
                ApiName = ApiName,
                ApiVersion = ApiVersion,
                Actions = ExtractActions(),
                Submits = ExtractSubmits()
            };

            return plugin;
        }

        public IEnumerable<ParameterAttribute> GetActionParameters(string name)
        {
            return ExtractActions().FirstOrDefault(t => t.Name == name)?.Parameters ?? [];
        }

        protected Task<string?> SendFunctionCard(
            ITurnContext turnContext, 
            string actionName,
            Dictionary<string, object> parameters)
        {
            ResultCardData resultCardData = new(new CultureInfo(turnContext.Activity.Locale))
            {
                Header = actionName?.Split(".").Last(),
                SubTitle = SourceName,
                Parameters = parameters?.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value?.ToString() ?? string.Empty))
            };

            return proactiveMessageService.SendOrUpdateCardAsync(
                                    turnContext.Activity.GetConversationReference(),
                                    () => FunctionCards.FunctionResultCardTemplate.RenderAdaptiveCard(resultCardData),
                                    null,
                                    CancellationToken.None);
        }

        protected string? VerifyParameters(string actionName,
                  Dictionary<string, object> parameters, List<ParameterAttribute>? actionParams = null)
        {
            var paramAttributes = actionParams ?? GetActionParameters(actionName)?.ToList() ?? [];

            if (paramAttributes.Any(f => f.Required && !parameters.ContainsKey(f.Name)))
            {
                var missingParams = paramAttributes.Where(t => t.Required && !parameters.ContainsKey(t.Name));
                var missingReadonlyProps = string.Join(", ", missingParams.Select(a => a.Name));

                return $"Required parameters missing: {missingReadonlyProps}";
            }

            return null;
        }

        protected async Task<string> SendConfirmationCard(ITurnContext turnContext, string actionName,
                  Dictionary<string, object> parameters, List<ParameterAttribute>? actionParams = null)
        {
            actionParams ??= GetActionParameters(actionName)?.ToList();

            var missingParams = VerifyParameters(actionName, parameters, actionParams);

            if (missingParams != null)
            {
                return missingParams;
            }

            await proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                                                                () => FunctionCards.CreateConfirmationCard(actionName, SourceName, parameters, actionParams),
                                                                 null, CancellationToken.None);

            return "An adaptive card has been presented. The user can submit the card to perform the action.";
        }

        protected Task SendConfirmedCard(ITurnContext turnContext, string actionName,
                        Dictionary<string, object>? parameters, CancellationToken cancellationToken)
        {
            ConfirmedCardData confirmedCardData = new(new CultureInfo(turnContext.Activity.Locale))
            {
                Header = actionName?.Split(".").Last(),
                SubTitle = SourceName,
                Parameters = parameters?.ToKeyValueList(),
                Submitted = $"{DateTime.Now} by {turnContext.Activity.From.Name}"
            };

            return proactiveMessageService.SendOrUpdateCardAsync(turnContext.Activity.GetConversationReference(),
                                 () => FunctionCards.FunctionConfirmedCardTemplate.RenderAdaptiveCard(confirmedCardData),
                                  turnContext.Activity.ReplyToId, cancellationToken);
        }

        protected async Task UpdateFunctionCard(ITurnContext turnContext, TeamsAIssistantState turnState, string actionName,
            Dictionary<string, object> parameters, string jsonContent, string? replyId)
        {
            if (turnState.CreateFunctionExports.HasValue && turnState.CreateFunctionExports.Value && replyId != null)
            {
                var data = await jsonContent.ConvertJsonToCsv();

                if (data != null)
                {
                    var filename = $"{actionName}-{DateTime.Now.Ticks}.csv";
                    var result = await driveRepository.UploadDriveFileAsync(turnContext.Activity.Recipient.Name, filename, data);

                    if (result != null)
                    {
                        ResultCardData resultCardData = new(new CultureInfo(turnContext.Activity.Locale))
                        {
                            Header = actionName?.Split(".").Last(),
                            SubTitle = SourceName,
                            Parameters = parameters.ToKeyValueList(),
                            Filename = filename,
                            ExportUrl = result
                        };

                        await proactiveMessageService.SendOrUpdateCardAsync(
                                    turnContext.Activity.GetConversationReference(),
                                    () => FunctionCards.FunctionResultCardTemplate.RenderAdaptiveCard(resultCardData),
                                    replyId,
                                    CancellationToken.None);
                    }
                }
            }
        }

        private List<PluginAction> ExtractActions()
        {
            var tools = new List<PluginAction>();
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var actionAttribute = method.GetCustomAttribute<ActionAttribute>();
                var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
                var parameterAttributes = method.GetCustomAttributes<ParameterAttribute>();

                if (actionAttribute != null)
                {
                    var tool = new Tool
                    {
                        Type = Tool.FUNCTION_CALLING_TYPE,
                        Function = new Function
                        {
                            Name = actionAttribute.Name,
                            Description = descriptionAttribute?.Description,
                            Parameters = ExtractParametersFromAttributes(parameterAttributes)
                        }
                    };

                    tools.Add(new PluginAction()
                    {
                        Name = actionAttribute.Name,
                        Tool = tool,
                        Parameters = parameterAttributes
                    });
                }
            }

            return tools;
        }

        private List<(string name, ActionSubmitHandler<TeamsAIssistantState> handler)> ExtractSubmits()
        {
            var tools = new List<(string name, ActionSubmitHandler<TeamsAIssistantState> handler)>();
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var actionAttribute = method.GetCustomAttribute<SubmitAttribute>();

                if (actionAttribute != null)
                {
                    var handler = (ActionSubmitHandler<TeamsAIssistantState>)Delegate.CreateDelegate(
                        typeof(ActionSubmitHandler<TeamsAIssistantState>),
                        this,
                        method);

                    tools.Add((method.Name, handler));
                }
            }

            return tools;
        }

        private static Dictionary<string, object> ExtractParametersFromAttributes(IEnumerable<ParameterAttribute> parameterAttributes)
        {
            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var param in parameterAttributes)
            {
                var paramDetails = new Dictionary<string, object>
                {
                    { "type", param.ParamType }
                };

                if (!string.IsNullOrEmpty(param.Description))
                    paramDetails.Add("description", param.Description);

                if (param.Minimum.HasValue)
                    paramDetails.Add("minimum", param.Minimum);

                if (param.Maximum.HasValue)
                    paramDetails.Add("maximum", param.Maximum);

                if (param.MaxLength > 0)
                    paramDetails.Add("maxLength", param.MaxLength);

                if (param.EnumValues != null && param.EnumValues.Length != 0)
                    paramDetails.Add("enum", param.EnumValues);

                properties.Add(param.Name, paramDetails);

                if (param.Required)
                {
                    required.Add(param.Name);
                }
            }

            return new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "properties", properties },
                    { "required", required ?? [] }
                };
        }

    }

}
