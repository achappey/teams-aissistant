using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using TeamsAIssistant.Constants;

namespace TeamsAIssistant.Extensions
{
    public static class StringExtensions
    {
        public static string ToListString(this IEnumerable<string>? items)
        {
            return string.Join(",", items ?? []);
        }

        public static List<string>? ToStringList(this string? text)
        {
            return text?.Split(",").Where(t => !string.IsNullOrEmpty(t)).ToList();
        }

        public static dynamic? ExtractFirstData(this IEnumerable<IEnumerable<IEnumerable<dynamic>>> data)
        {
            if (data == null) return null;

            var firstNestedList = data.FirstOrDefault()?.FirstOrDefault();
            if (firstNestedList == null || !firstNestedList.Any()) return null;

            return new
            {
                Value = firstNestedList.ElementAtOrDefault(1),
                Delta = firstNestedList.ElementAtOrDefault(0),
                DeltaCompare = firstNestedList.ElementAtOrDefault(2)
            };
        }

        public static (string Hostname, string Path, string PageName) ExtractSharePointValues(this string sharePointUrl)
        {
            // Extracting the hostname, site path, and page name from the given URL
            var uri = new Uri(sharePointUrl);
            string hostname = uri.Host;
            string[] pathSegments = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Assuming the path segment after "sites" is the required path
            int siteIndex = Array.IndexOf(pathSegments, "sites");
            string path = siteIndex >= 0 && pathSegments.Length > siteIndex + 1 ? pathSegments[siteIndex + 1] : string.Empty;

            // Assuming the page name is the last segment in the URL
            string pageName = pathSegments.Length > 0 ? pathSegments[pathSegments.Length - 1] : string.Empty;

            return (Hostname: hostname, Path: path, PageName: pageName);
        }


        public static NameValueCollection BuildQueryString(this Dictionary<string, object>? filters)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                    {
                        queryString[$"{filter.Key}"] = $"{filter.Value}";
                    }
                }
            }

            return queryString;
        }

        public static string ToSubmitVerb(this string actionName)
        {
            return $"{actionName}Submit";
        }

        public static int? GetSkip(this IDictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("skip", out var value) && int.TryParse(value?.ToString(), out int skip))
            {
                return skip;
            }

            return null;
        }

        public static int? GetTop(this IDictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("top", out var value) && int.TryParse(value?.ToString(), out int top))
            {
                return top;
            }

            return null;
        }

        public static string Sanitize(this string? text)
        {
            return text is null ? string.Empty : text.Replace("\n", " ").Replace("\r", " ").Trim();
        }

        public static IEnumerable<string> ExtractAllHrefs(this string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent)) return [];

            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            return document.DocumentNode.Descendants("a")
                    .Select(anchorTag => anchorTag.GetAttributeValue("href", string.Empty))
                    .Where(hrefValue => !string.IsNullOrEmpty(hrefValue));
        }

        public static string FindNameFromUrl(this string url)
        {
            var fileName = url.GetFilenameFromUrl();

            if (string.IsNullOrEmpty(fileName))
            {
                return url.GetFriendlyNameFromUrl();
            }

            return fileName;
        }


        public static string UrlToFileName(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return "default_filename";

            var uri = new Uri(url);
            var baseName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
            var extension = Path.GetExtension(uri.AbsolutePath);

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = uri.Host.Replace(".", "") + uri.AbsolutePath.Replace("/", "");
            }

            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            baseName = Regex.Replace(baseName, invalidRegStr, "_");
            baseName = baseName.Replace(".", "");

            if (extension == null || !FileTypes.CodeInterpreterExtensions.Contains(extension))
            {
                extension = ".html";
            }

            int maxBaseNameLength = 255 - extension.Length;
            baseName = baseName.Length <= maxBaseNameLength ? baseName : baseName[..maxBaseNameLength];

            var safeName = baseName + extension;

            return safeName;
        }

        public static string GetFilenameFromUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return string.Empty;
            }

            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            var filename = queryParameters["filename"];
            if (!string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            return Path.GetFileName(uri.LocalPath);

        }

        public static string GetFriendlyNameFromUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return string.Empty;
            }

            string path = uri.AbsolutePath.TrimEnd('/');
            if (string.IsNullOrEmpty(path) || path.Equals("/"))
            {
                return uri.Host.Replace("www.", "").Replace(".com", "").Replace(".org", "").Replace(".net", "");
            }

            string[] segments = path.Split('/');
            return segments.Last().Replace("%20", " ");
        }

    }
}