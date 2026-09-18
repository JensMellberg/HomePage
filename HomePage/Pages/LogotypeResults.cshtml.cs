using HomePage.Data;
using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{ 
    public class LogotypeResultsModel(LogotypeResultRepository resultRepository, AppDbContext dbContext, SignInRepository signInRepository) 
        : BasePage(signInRepository)
    {
        public required List<LogotypeDayResult> PreviousResults { get; set; }

        public required Dictionary<string, string> DisplayNames { get; set; }

        public LogotypeDayResult? TodaysResult { get; set; }

        public IActionResult OnGet()
        {
            var allDayResults = resultRepository.GetResultsByDay();
            DisplayNames = dbContext.UserInfo.ToDictionary(x => x.UserName, x => x.DisplayName);
            var loggedInPerson = LoggedInPerson?.UserName;
            PreviousResults = allDayResults.previous;
            TodaysResult = allDayResults.current;
            return Page();
        }
    }
}
