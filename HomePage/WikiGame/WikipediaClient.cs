using System.Web;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HomePage.WikiGame
{
    public class WikipediaClient
    {
        private static readonly HttpClient client = SetupClient();

        public static string GetRandomTitle(string language)
        {
            var url = $"{BaseUrl(language)}/api/rest_v1/page/random/summary";
            var json = client.GetStringAsync(url).Result;
            var result = JsonConvert.DeserializeObject<WikiPageModel>(json);
            if (result?.title is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return result!.title;
        }

        public static (string title, string summary) GetPageInfo(string title, string language)
        {
            var url = $"{BaseUrl(language)}/api/rest_v1/page/summary/{title}";
            var json = client.GetStringAsync(url).Result;
            var result = JsonConvert.DeserializeObject<WikiPageModel>(json);
            if (result?.title is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return (result!.title, result!.extract);
        }

        public static string GetPageContent(string title, string language)
        {
            var normalized = title.Replace(' ', '_');
            var url = $"{BaseUrl(language)}/api/rest_v1/page/mobile-html/{HttpUtility.UrlEncode(normalized)}";
            var content = client.GetStringAsync(url).Result;
            return content;
        }

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
        }
    }
}
