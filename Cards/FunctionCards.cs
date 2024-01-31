using AdaptiveCards;
using TeamsAIssistant.Extensions;
using TeamsAIssistant.Attributes;
using TeamsAIssistant.Models.Cards;
using System.Globalization;

namespace TeamsAIssistant.AdaptiveCards;

public static class FunctionCards
{
    public const string FunctionResultCardTemplate = "Resources/Cards/functionResultCardTemplate.json";
    public const string FunctionConfirmedCardTemplate = "Resources/Cards/functionConfirmedCardTemplate.json";

    public static AdaptiveCard CreateConfirmationCard(string functionName, string source,
        IDictionary<string, object> parameters,
        List<ParameterAttribute>? actionParams)
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));
        card.Body.Add(functionName.ToAdaptiveCardHeader(source));

        if (actionParams != null)
        {
            var readOnlyParams = parameters.Where(a => actionParams.Any(r => r.Name == a.Key)
                    && actionParams.First(r => r.Name == a.Key).ReadOnly && actionParams.First(r => r.Name == a.Key).Visible);

            var factSet = new AdaptiveFactSet();

            if (readOnlyParams != null)
            {
                factSet.Facts.AddRange(readOnlyParams.Select(fact =>
                  new AdaptiveFact(fact.Key, fact.Value?.ToString() ?? string.Empty)));
            }

            card.Body.Add(factSet);

            foreach (var param in parameters)
            {
                string paramValue = string.Empty;
                var actionParam = actionParams?.FirstOrDefault(r => r.Name == param.Key);

                if (actionParam != null)
                {
                    card.Body.AddRange(actionParam.ToAdaptiveElements(param.Value?.ToString()));
                }

            }
        }

        card.Actions.Add("Submit".ToAdaptiveSubmitAction(string.Join("", functionName.Split(".")).ToSubmitVerb()));

        return card;
    }
}

public class ConfirmedCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
{
    public IEnumerable<KeyValuePair<string, string>>? Parameters { get; set; }
    public string? Submitted { get; set; }
}

public class ResultCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
{
    public IEnumerable<KeyValuePair<string, string>>? Parameters { get; set; }
    public string? ExportUrl { get; set; }
    public string? Filename { get; set; }
    public bool ShowExportActions { get { return ExportUrl != null; } }
    public string? AddExcelToChatText => GetResourceString("AddExcelToChatText");
    public string? OpenInExcelText => GetResourceString("OpenInExcelText");
    
    
}