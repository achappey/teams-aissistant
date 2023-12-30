using AdaptiveCards;
using Microsoft.Teams.AI.AI.OpenAI.Models;

public static class AdaptiveCardCreator
{
    public static AdaptiveCard CreateFileListCard(List<TeamsAIssistant.Models.File> files)
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));

        card.Body.Add(new AdaptiveTextBlock()
        {
            Text = "Files",
            Weight = AdaptiveTextWeight.Bolder,
            Size = AdaptiveTextSize.Medium
        });

        var container = new AdaptiveContainer();
        foreach (var file in files)
        {
            var columnSet = new AdaptiveColumnSet();

            columnSet.Columns.Add(new AdaptiveColumn()
            {
                Items = { new AdaptiveTextBlock() { Text = file.Filename, Wrap = true } }
            });

            columnSet.Columns.Add(new AdaptiveColumn()
            {
                Items = { new AdaptiveTextBlock() { Text = $"{file.Bytes} bytes", Wrap = true } }
            });

            columnSet.Columns.Add(new AdaptiveColumn()
            {
                Items = { new AdaptiveTextBlock() { Text = $"{file.CreatedAt}", Wrap = true } }
            });

            container.Items.Add(columnSet);
        }

        card.Body.Add(container);

        return card;
    }

    public static AdaptiveCard CreateAssistantInfoCard(Assistant assistant)
    {
        var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

        card.Body.Add(new AdaptiveTextBlock
        {
            Text = $"{assistant.Name}",
            Size = AdaptiveTextSize.Medium,
            Weight = AdaptiveTextWeight.Bolder
        });

        var factSet = new AdaptiveFactSet();
        factSet.Facts.Add(new AdaptiveFact("Model", assistant.Model));
        factSet.Facts.Add(new AdaptiveFact("Created at", DateTimeOffset.FromFileTime(assistant.CreatedAt).ToString("g")));
        factSet.Facts.Add(new AdaptiveFact("Description", assistant.Description ?? "Not available"));
        factSet.Facts.Add(new AdaptiveFact("File count", assistant.FileIds?.Count.ToString() ?? "Not available"));
        card.Body.Add(factSet);

        card.Body.Add(new AdaptiveTextBlock
        {
            Text = $"Instructions",
            Size = AdaptiveTextSize.Default,
            Weight = AdaptiveTextWeight.Bolder
        });


        card.Body.Add(new AdaptiveTextBlock
        {
            Text = $"{assistant.Instructions ?? "Not available"}",
            Wrap = true
        });

        return card;
    }
}
