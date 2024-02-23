using System.Globalization;
using Microsoft.KernelMemory;
using TeamsAIssistant.Models.Cards;

namespace TeamsAIssistant.AdaptiveCards
{
    public static class CitationCard
    {
        public const string CitationCardTemplate = "Resources/Cards/citationCardTemplate.json";
    }

    public class CitationCardData(CultureInfo cultureInfo) : CardData(cultureInfo)
    {
        public Citation? Citation { get; set; }
     
        public string? RelevanceText => GetResourceString("RelevanceText");
        public string? QuoteText => GetResourceString("QuoteText");
        

    }
}