using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using TeamsAIssistant.Attributes;

namespace TeamsAIssistant.Extensions
{
    public static class CardExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> ToKeyValueList(this IDictionary<string, object> dict)
        {
            return dict.Select(kv => new KeyValuePair<string, string>(kv.Key!.ToString(), kv.Value?.ToString() ?? string.Empty));
        }

        public static AdaptiveCard RenderAdaptiveCard<T>(this string jsonFilePath, T data)
        {
            string jsonString = File.ReadAllText(jsonFilePath);

            AdaptiveCardTemplate template = new(jsonString);

            string cardJson = template.Expand(data);

            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            return card;
        }

        public static Dictionary<string, object>? ExcludeVerb(this Dictionary<string, object>? parameters)
        {
            return parameters?.Where(t => t.Key != "verb").ToDictionary(t => t.Key, t => t.Value);
        }

        public static object GetFormValue(this KeyValuePair<string, object> param, IEnumerable<ParameterAttribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a => a.Name == param.Key);

            if (attribute != null)
            {
                return attribute.ParamType switch
                {
                    "boolean" => bool.TryParse(param.Value?.ToString(), out bool result) && result,
                    _ => param.Value,
                };
            }

            return param.Value;
        }

        public static AdaptiveSubmitAction ToAdaptiveSubmitAction(this string? title, string verb)
        {
            return new AdaptiveSubmitAction
            {
                Title = title,
                Data = new { verb }
            };
        }

        public static AdaptiveChoice ToAdaptiveChoice(this string? choice)
        {
            return new AdaptiveChoice
            {
                Value = choice,
                Title = choice
            };
        }

        public static AdaptiveContainer ToAdaptiveCardHeader(this string? header, string? subTitle = null, string? sourceUrl = null)
        {
            var titleColumn = new AdaptiveColumn()
            {
                Width = "stretch",
                Items =
                [
                    new AdaptiveTextBlock
                    {
                        Text = header?.Split(".").Last(),
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder
                    }
                ]
            };

            if (subTitle != null)
            {
                titleColumn.Items.Add(new AdaptiveTextBlock()
                {
                    Text = subTitle,
                    Size = AdaptiveTextSize.Small
                });
            }

            var columns = new List<AdaptiveColumn> { titleColumn };

            if (sourceUrl != null)
            {
                var sourceColumn = new AdaptiveColumn()
                {
                    Width = "auto",
                    Items =
                    [
                        new AdaptiveActionSet()
                        {
                            Actions =
                            [
                                new AdaptiveOpenUrlAction()
                                {
                                    Title = "Source",
                                    Url = new Uri(sourceUrl)
                                }
                            ]
                        }
                    ]
                };

                columns.Add(sourceColumn);
            }

            var container = new AdaptiveContainer()
            {
                Style = AdaptiveContainerStyle.Emphasis,
                Items =
                [
                    new AdaptiveColumnSet()
                    {
                        Columns = columns
                    }
                ]
            };

            return container;
        }

        public static IEnumerable<AdaptiveElement> ToAdaptiveElements(this ParameterAttribute parameter, string? value)
        {
            return [
                parameter.ToAdaptiveInput(value)
            ];
        }

        public static AdaptiveElement ToAdaptiveInput(this ParameterAttribute parameter, string? value)
        {
            var isVisible = !parameter.ReadOnly && parameter.Visible;

            switch (parameter.ParamType)
            {
                case "number":
                    return new AdaptiveNumberInput()
                    {
                        Id = parameter.Name,
                        Value = value != null ? double.Parse(value) : double.NaN,
                        Label = parameter.Name,
                        Min = parameter.Minimum ?? double.NaN,
                        Max = parameter.Maximum ?? double.NaN,
                        IsVisible = isVisible,
                        IsRequired = parameter.Required
                    };
                case "boolean":
                    return new AdaptiveToggleInput()
                    {
                        Id = parameter.Name,
                        Value = value,
                        Label = parameter.Name,
                        IsVisible = isVisible,
                        IsRequired = parameter.Required
                    };
                case "date-time":
                    return new AdaptiveDateInput()
                    {
                        Id = parameter.Name,
                        Value = value,
                        Label = parameter.Name,
                        IsVisible = isVisible,
                        IsRequired = parameter.Required,
                    };
                default:
                    if (parameter.EnumValues?.Length > 0)
                    {
                        return new AdaptiveChoiceSetInput()
                        {
                            Id = parameter.Name,
                            Value = value,
                            Label = parameter.Name,
                            IsVisible = isVisible,
                            IsRequired = parameter.Required,
                            Choices = parameter.EnumValues.Select(ToAdaptiveChoice).ToList()
                        };
                    }

                    return new AdaptiveTextInput()
                    {
                        Id = parameter.Name,
                        Value = value,
                        Style = parameter.Format == "email" ? AdaptiveTextInputStyle.Email
                            : parameter.Format == "tel" ? AdaptiveTextInputStyle.Tel
                            : parameter.Format == "uri" ? AdaptiveTextInputStyle.Url
                            : parameter.Format == "password" ? AdaptiveTextInputStyle.Password
                            : AdaptiveTextInputStyle.Text,
                        Label = parameter.Name,
                        MaxLength = parameter.MaxLength ?? 0,
                        IsVisible = isVisible,
                        IsRequired = parameter.Required,
                        IsMultiline = parameter.Multiline
                    };
            }
        }

        public static IMessageActivity ToAdaptiveCardAttachment(this AdaptiveCard card)
        {
            return MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            });
        }
    }
}