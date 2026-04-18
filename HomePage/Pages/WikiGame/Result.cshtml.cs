using System;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    public class ResultModel(WikiGameRepository wikiGameRepository, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public string PersonClass { get; set; }

        public int Score { get; set; }

        public List<string> PathTaken { get; set; }

        public IActionResult OnGet(DateTime date, string user)
        {
            var result = wikiGameRepository.GetUserResultForDate(user, date);
            if (result == null || (date >= DateHelper.DateNow && !wikiGameRepository.UserHasResultToday(LoggedInPerson?.UserName)))
            {
                logger.Error("Tried to access wiki game result that was not allowed. " + date, LoggedInPerson?.UserName);
                return new RedirectToPageResult("/AccessDenied");
            }

            PersonClass = result.UserName == Person.Jens.UserName || result.UserName == Person.Anna.UserName ? result.UserName.ToLower() : "unknown";
            Score = result.Steps;
            PathTaken = result.PathTaken.Select(x => x.Replace("_", " ")).ToList();
            return Page();
        }
    }
}
