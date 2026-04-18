using System.Web;
using Newtonsoft.Json;

namespace HomePage.WikiGame
{
    public class WikipediaClient
    {
        private static readonly HttpClient client = SetupClient();

        public static string GetRandomTitle()
        {
            var url = "https://en.wikipedia.org/api/rest_v1/page/random/summary";
            var json = client.GetStringAsync(url).Result;
            var result = JsonConvert.DeserializeObject<RandomPageModel>(json);
            if (result?.title is null)
            {
                throw new Exception("Failed to get wikipedia page.");
            }

            return result!.title;
        }

        public static string GetPageContent(string title)
        {
            var normalized = title.Replace(' ', '_');
            var url = $"https://en.wikipedia.org/api/rest_v1/page/mobile-html/{HttpUtility.UrlEncode(normalized)}";
            var content = client.GetStringAsync(url).Result;
            return content;
        }

        private static HttpClient SetupClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "WikiGame/1.0 (jens_mellberg@hotmail.com)");
            return client;
        }

        private class RandomPageModel
        {
            public string title { get; set; }
        }
    }
}
