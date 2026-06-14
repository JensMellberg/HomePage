using HomePage.Repositories;
using HomePage.WikiGame;
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

        public string BackbuttonDisplay { get; set; }

        public string CurrentTitle { get; set; }

        public IActionResult OnGet()
        {
            var currentPage = wikiGameRespository.GetCurrentUserPage(LoggedInPerson!.UserName);
            PageHtml = currentPage.html;
            PreviousClicks = currentPage.steps;
            HasWon = currentPage.allowedLinks.Count == 0;
            BackbuttonDisplay = currentPage.lastNavigationId.HasValue && !HasWon ? "block;" : "none;";
            CurrentTitle = currentPage.title;
            return Page();
        }

        public IActionResult OnPost(string pageName)
        {
            var redirectResult = GetPotentialClientRedirectResult(false, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }


            WikiGameNavigationResult? navigateResult;
            if (pageName == "GOBACKONCE")
            {
                navigateResult = wikiGameRespository.NavigateBackwards(LoggedInPerson!.UserName);
            }
            else
            {
                navigateResult = wikiGameRespository.NavigateToPage(pageName, LoggedInPerson!.UserName);
            }

            if (navigateResult == null)
            {
                return Utils.CreateErrorClientResult("Du får inte gå till den länken.");
            }

            logger.Information($"User {LoggedInPerson!.UserName} navigated to page {pageName}. Currently on {navigateResult.Steps} steps.", LoggedInPerson!.UserName);
            return Utils.CreateClientResult(new
            {
                navigateResult.Html,
                navigateResult.Steps,
                navigateResult.IsWin,
                navigateResult.CanGoBack,
                navigateResult.Title
            });
        }
    }
}
