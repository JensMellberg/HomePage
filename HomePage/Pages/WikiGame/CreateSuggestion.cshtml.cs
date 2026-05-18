using System.Text;
using System.Web;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using HomePage.WikiGame;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    [RequireLogin]
    [IgnoreAntiforgeryToken]
    public class CreateSuggestionModel(WikiGameRepository wikiGameRepository,
        SignInRepository signInRepository,
        DatabaseLogger logger,
        AppDbContext dbContext) : BasePage(signInRepository)
    {
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost(string url, bool isValidate)
        {
            if (!isValidate)
            {
                return RedirectToPage("Suggestions");
            }

            var redirectResult = GetPotentialClientRedirectResult(false, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var languange = "en";
            if (url.Contains("sv.wikipedia"))
            {
                languange = "sv";
            }

            const string wikiPath = "/wiki/";
            if (url.Contains(wikiPath))
            {
                url = HttpUtility.UrlDecode(url);
                var index = url.IndexOf(wikiPath);
                index += wikiPath.Length;
                var sb = new StringBuilder();
                while (index < url.Length && url[index] != '.' && url[index] != '/')
                {
                    sb.Append(url[index]);
                    index++;
                }

                url = sb.ToString();
            }

            if (dbContext.WikiGameSuggestions.Any(x => x.Title == url)) {
                return Utils.CreateClientResult(new { success = false, reason = "Exists" });
            }

            try
            {
                WikipediaClient.GetPageInfo(url, languange);
            }
            catch (Exception e)
            {
                logger.Error($"No wikipedia page found for suggested url {url} Error: {e.Message}.", LoggedInPerson?.UserName);
                return Utils.CreateClientResult(new { success = false, reason = "NotFound" });
            }

            dbContext.WikiGameSuggestions.Add(new WikiGameSuggestion {
                Creator = LoggedInPerson!.UserName,
                Title = url.Replace(' ', '_'),
                Votes = 0,
                Language = languange
            });
            dbContext.SaveChanges();

            return Utils.CreateClientResult(new { success = true });
        }
    }
}
