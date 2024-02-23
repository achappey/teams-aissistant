using Microsoft.KernelMemory;
using Microsoft.Teams.AI.AI.Planners;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Extensions
{
    public static class KernelMemoryExtensions
    {
        public static string[] GetDocumentIdParts(this Citation citation)
        {
            return citation.DocumentId.Split("-...-");
        }

        public static string? GetPageId(this Citation citation)
        {
            var splitted = citation.GetDocumentIdParts();

            return splitted.ElementAt(1);
        }

        public static string? GetDriveId(this Citation citation)
        {
            var splitted = citation.GetDocumentIdParts();

            return splitted.ElementAt(0).Replace(".---.", "!");
        }

        public static string? GetDriveItemId(this Citation citation)
        {
            var splitted = citation.GetDocumentIdParts();

            return splitted.ElementAt(1);
        }

        public static string? GetChannelId(this Citation citation)
        {
            var splitted = citation.GetDocumentIdParts();

            return splitted.ElementAt(1).Replace(".---.", ":").Replace(".----.", "@");
        }

        public static Plan AddCitations(this Plan plan, IEnumerable<Citation>? citations, int? maxCitations = null)
        {
            if (citations != null)
            {
                var items = maxCitations.HasValue && maxCitations >= 0 ? citations.Take(maxCitations.Value) : citations;

                plan.Commands.AddRange(items.Select(t => new PredictedDoCommand(AIConstants.CitationActionName,
                   new() { { "citation", t } })));
            }

            return plan;
        }

    }
}
