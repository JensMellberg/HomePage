using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    [RequireLogin]
    [IgnoreAntiforgeryToken]
    public class CurrentGameModel(WikiGameRepository wikiGameRespository, DatabaseLogger logger, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public string PageHtml { get; set; }

        public int PreviousClicks { get; set; }

        public bool HasWon { get; set; }
        public IActionResult OnGet()
        {
            var currentPage = wikiGameRespository.GetCurrentUserPage(LoggedInPerson!.UserName);
            PageHtml = currentPage.html;
            PreviousClicks = currentPage.steps;
            HasWon = currentPage.allowedLinks.Count == 0;
            return Page();
        }

        public IActionResult OnPost(string pageName)
        {
            var redirectResult = GetPotentialClientRedirectResult(false, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var navigateResult = wikiGameRespository.NavigateToPage(pageName, LoggedInPerson!.UserName);
            if (navigateResult == null)
            {
                return Utils.CreateErrorClientResult("Du får inte gå till den länken.");
            }

            logger.Information($"User {LoggedInPerson!.UserName} navigated to page {pageName}. Currently on {navigateResult.Value.steps} steps.", LoggedInPerson!.UserName);
            return Utils.CreateClientResult(new
            {
                navigateResult.Value.html,
                navigateResult.Value.steps,
                navigateResult.Value.isWin
            });
        }
    }
}
