
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Extensions
{
    public static class CsvExtensions
    {

        public static Task<byte[]?> ConvertJsonToCsv(this string jsonInput)
        {
            var objects = ParseJson(jsonInput);
            var flattenedObjects = FlattenJsonObjects(objects);
            var nonEmptyColumns = IdentifyNonEmptyColumns(flattenedObjects);
            return GenerateCsv(flattenedObjects, nonEmptyColumns);
        }

        private static List<Dictionary<string, object>>? ParseJson(string jsonInput)
        {
            var token = JToken.Parse(jsonInput);
            return token.Type switch
            {
                JTokenType.Array => token.ToObject<List<Dictionary<string, object>>>(),
                JTokenType.Object => [token.ToObject<Dictionary<string, object>>() ?? []],
                _ => throw new ArgumentException("The provided JSON string is neither a JSON object nor an array of JSON objects."),
            };
        }

        private static List<Dictionary<string, object>>? FlattenJsonObjects(List<Dictionary<string, object>>? objects)
        {
            return objects?.Select(obj => obj.FlattenJson()).ToList();
        }

        private static HashSet<string> IdentifyNonEmptyColumns(List<Dictionary<string, object>>? objects)
        {
            var nonEmptyColumns = new HashSet<string>();

            foreach (var dict in objects ?? [])
            {
                foreach (var key in dict.Keys)
                {
                    if (dict[key] != null && dict[key].ToString() != string.Empty)
                    {
                        nonEmptyColumns.Add(key);
                    }
                }
            }
            
            return nonEmptyColumns;
        }

        private static async Task<byte[]?> GenerateCsv(List<Dictionary<string, object>>? flattenedObjects, HashSet<string> nonEmptyColumns)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => true,
                Quote = '\"'
            };

            if (flattenedObjects == null)
            {
                return null;
            }

            // Assuming an average of 50 bytes per field, adjust as needed.
            int estimatedSize = flattenedObjects.Count * nonEmptyColumns.Count * 50;
            await using var memoryStream = new MemoryStream(estimatedSize);
            await using var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true);
            await using var csv = new CsvWriter(writer, config);

            // Write CSV headers
            foreach (var header in nonEmptyColumns)
            {
                csv.WriteField(header);
            }
            csv.NextRecord();

            // Write CSV records
            foreach (var record in flattenedObjects)
            {
                foreach (var column in nonEmptyColumns)
                {
                    csv.WriteField(record.TryGetValue(column, out var value) ? value : null);
                }
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        private static Dictionary<string, object> FlattenJson(this Dictionary<string, object>? dict, string parentKey = "")
        {
            var flattened = new Dictionary<string, object>();

            if (dict != null)
            {
                foreach (var keyValuePair in dict)
                {
                    var key = string.IsNullOrEmpty(parentKey) ? keyValuePair.Key : $"{parentKey}.{keyValuePair.Key}";

                    if (keyValuePair.Value is JObject nestedObject)
                    {
                        var nestedFlattened = nestedObject.ToObject<Dictionary<string, object>>().FlattenJson(key);
                        foreach (var nestedEntry in nestedFlattened)
                        {
                            flattened[nestedEntry.Key] = nestedEntry.Value;
                        }
                    }
                    else if (keyValuePair.Value is JArray arrayValue)
                    {
                        //        flattened[key] = arrayValue.ToString();
                    }
                    else
                    {
                        flattened[key] = keyValuePair.Value;
                    }
                }
            }


            return flattened;
        }
    }
}