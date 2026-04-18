using HomePage.Data;
using HomePage.Repositories;
using HomePage.WikiGame;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    public class ResultsModel(AppDbContext dbContext, WikiGameRepository wikiGameRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Dictionary<string, string> AccountNames { get; set; }
        public WikiGameDayResult? TodayResult { get; set; }

        public List<WikiGameDayResult> PreviousResults { get; set; }
        public bool IsFinished { get; set; }
        public IActionResult OnGet()
        {
            wikiGameRepository.GetStartPageTitle(DateHelper.DateNow);
            AccountNames = dbContext.UserInfo.ToDictionary(x => x.UserName, x => x.DisplayName);
            var results = wikiGameRepository.GetAllResults();
            TodayResult = results.FirstOrDefault(x => x.Date == DateHelper.DateNow);
            PreviousResults = results.Where(x => x.Date != DateHelper.DateNow).ToList();
            IsFinished = wikiGameRepository.UserHasResultToday(LoggedInPerson?.UserName);

            return Page();
        }
    }
}
