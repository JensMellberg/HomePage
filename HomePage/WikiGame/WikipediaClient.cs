using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace HomePage.WikiGame
{
    public class WikipediaClient
    {
        private static readonly HttpClient client = SetupClient();

        public static WikipediaPageMetadata GetRandomPage(string language)
        {
            var url = $"{BaseUrl(language)}/api/rest_v1/page/random/summary";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<WikiPageModel>(json);
            if (result is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return new WikipediaPageMetadata(result.pageid, result.title);
        }

        public static List<WikipediaPageMetadata> GetOutgoingLinksFromPage(long? pageId, string title, string language, DatabaseLogger logger, out long totalRequests)
        {
            return CollectLinks((gplcontinue) => GetOutgoingLinksResult(pageId, title, language, gplcontinue, logger), int.MaxValue, out totalRequests);
        }

        public static List<WikipediaPageMetadata> GetIncomingLinksToPage(long? pageId, string title, string language, DatabaseLogger logger, out long totalRequests)
        {
            return CollectLinks((gblcontinue) => GetIncomingLinksResult(pageId, title, language, gblcontinue, logger), 5000, out totalRequests);
        }

        public static WikipediaPageMetadataWithSummary GetPageInfo(string title, string language)
        {
            var url = $"{BaseUrl(language)}/api/rest_v1/page/summary/{title}";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<WikiPageModel>(json);
            if (result?.title is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return new(result!.pageid, result!.title, result!.extract);
        }

        public static WikipediaPageMetadataWithSummary GetPageInfoFromId(long pageId, string language)
        {
            var url = $"{BaseUrl(language)}/w/api.php?action=query&pageids={pageId}&prop=extracts&exintro=1&explaintext=1&format=json&formatversion=2";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<LinksResultApiModel>(json);
            if (result is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            var page = result.Query.pages.Single();
            return new(page.pageid, page.title, page.extract);
        }

        public static long GetPageViews(string title, string language)
        {
            var url = $"https://wikimedia.org/api/rest_v1/metrics/pageviews/per-article/{language}.wikipedia.org/all-access" + 
                 $"/user/{EncodeTitle(title)}/monthly/20250101/20251231";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<PageViewResult>(json);
            if (result?.items is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return result.items.Sum(x => x.views) / result.items.Length;
        }

        public static string GetPageContent(string title, string language)
        {
            var url = $"{BaseUrl(language)}/api/rest_v1/page/mobile-html/{EncodeTitle(title)}";
            var content = client.GetStringAsync(url).Result;
            return content;
        }

        private static GenericLinksResult GetOutgoingLinksResult(long? pageId, string title, string language, string? gplcontinue, DatabaseLogger logger)
        {
            var gplcontinueQs = gplcontinue != null ? $"&gplcontinue={gplcontinue}" : "";
            var url = $"{BaseUrl(language)}/w/api.php?action=query&{GetIdentifierQueryString()}&generator=links&gplnamespace=0" +
               $"&gpllimit=max&format=json&formatversion=2{gplcontinueQs}";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<LinksResultApiModel>(json);
            if (result is null)
            {
                throw new Exception("Failed to get outgoing links.");
            }

            return new GenericLinksResult
            {
                Continue = result.Continue?.gplcontinue,
                Pages = result.Query?.pages ?? []
            };

            string GetIdentifierQueryString() => pageId.HasValue 
                ? $"pageids={pageId}" 
                : $"titles={EncodeTitle(title)}";
        }

        private static string GetJsonWithRetries(string url, DatabaseLogger? logger = null)
        {
            HttpResponseMessage response;

            for (int attempt = 0; ; attempt++)
            {
                response = client.GetAsync(url).Result;

                if (response.StatusCode != HttpStatusCode.TooManyRequests)
                {
                    break;
                }

                if (attempt >= 5)
                {
                    throw new Exception("Wikipedia API rate limit exceeded.");
                }

                var delay = response.Headers.RetryAfter?.Delta
                            ?? TimeSpan.FromSeconds(Math.Pow(2, attempt));
                if (logger != null)
                {
                    logger.Warning($"Hit too many requests on wikipedia, waiting for {delay.TotalSeconds} seconds.", null);
                }

                Thread.Sleep(delay);
            }

            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;
            return json;
        }

        private static GenericLinksResult GetIncomingLinksResult(long? pageId, string title, string language, string? gblcontinue, DatabaseLogger logger)
        {
            var gblcontinueQs = gblcontinue != null ? $"&gblcontinue={gblcontinue}" : "";
            var url = $"{BaseUrl(language)}/w/api.php?action=query&{GetIdentifierQueryString()}&generator=backlinks&gblnamespace=0" +
               $"&gbllimit=max&format=json&formatversion=2{gblcontinueQs}";
            var json = GetJsonWithRetries(url);
            var result = JsonConvert.DeserializeObject<LinksResultApiModel>(json);
            if (result is null)
            {
                throw new Exception("Failed to get outgoing links.");
            }

            return new GenericLinksResult
            {
                Continue = result.Continue?.gblcontinue,
                Pages = result.Query?.pages ?? []
            };

            string GetIdentifierQueryString() => pageId.HasValue
                ? $"gblpageid={pageId}"
                : $"gbltitle={EncodeTitle(title)}";
        }

        private static List<WikipediaPageMetadata> CollectLinks(Func<string?, GenericLinksResult> getPages, int maxLinks, out long totalRequests)
        {
            totalRequests = 0;
            var links = new List<WikipediaPageMetadata>();
            string? continueString = null;
            do
            {
                var linksResult = getPages(continueString);
                links.AddRange(linksResult.Pages.Where(x => !x.missing).Select(x => new WikipediaPageMetadata(x.pageid, x.title)));
                continueString = linksResult.Continue;
                totalRequests++;
            }
            while (continueString != null && links.Count < maxLinks);

            return links;
        }

        private static string EncodeTitle(string title) => HttpUtility.UrlEncode(title.Replace(' ', '_'));

        private static string BaseUrl(string language) => $"https://{language}.wikipedia.org";

        private static HttpClient SetupClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "WikiGame/1.0 (jens_mellberg@hotmail.com)");
            return client;
        }

        private class WikiPageModel
        {
            public string title { get; set; }
            public string extract { get; set; }
            public long pageid { get; set; }
            public bool missing { get; set; }
        }

        private class LinksResultApiModel
        {
            public WikiQuery Query { get; set; }
            public ContinueApiModel? Continue { get; set; }
        }

        private class ContinueApiModel
        {
            public string gplcontinue { get; set; }
            public string gblcontinue { get; set; }
        }

        private class WikiQuery
        {
            public WikiPageModel[] pages { get; set; }
        }

        private class GenericLinksResult
        {
            public string? Continue;
            public WikiPageModel[] Pages { get; set; }
        }

        private class PageViewResult
        {
            public PageViewItem[] items { get; set; }
        }

        private class PageViewItem
        {
            public long views { get; set; }
        }
    }
}
