namespace TeamsAIssistant.Extensions
{
    public static class AIPricing
    {
        private static readonly Dictionary<string, ModelPricing> prices = new()
        {
                { "gpt-3.5-turbo-1106", new ModelPricing(0.0010, 0.0020) },
                { "gpt-3.5-turbo-0125", new ModelPricing(0.0005, 0.0015) },
                { "gpt-4-0125-preview", new ModelPricing(0.01, 0.03) },
                { "gpt-4", new ModelPricing(0.03, 0.06) },
                { "gpt-4-turbo-preview", new ModelPricing(0.01, 0.03) }
            };

        public static double? CalculateCost(string modelName, int inputTokens, int outputTokens)
        {
            if (prices.TryGetValue(modelName, out ModelPricing pricing))
            {
                return (pricing.InputPrice * ((double)inputTokens / 1000)) + (pricing.OutputPrice * ((double)outputTokens / 1000));
            }
            return null;
        }
    }

    public struct ModelPricing(double inputPrice, double outputPrice)
    {
        public double InputPrice = inputPrice;
        public double OutputPrice = outputPrice;
    }
}