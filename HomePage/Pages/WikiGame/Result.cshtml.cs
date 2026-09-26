using System;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    public class ResultModel(WikiGameRepository wikiGameRepository, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public int Score { get; set; }

        public required List<string> PathTaken { get; set; }
        
        public bool HasFinished { get; set; }

        public required Person Person { get; set; }

        public IActionResult OnGet(DateTime date, string user)
        {
            var result = wikiGameRepository.GetUserResultForDate(user, date);
            if (result == null || (date >= DateHelper.DateNow && !wikiGameRepository.UserHasFinishedToday(LoggedInPerson?.UserName)))
            {
                logger.Error("Tried to access wiki game result that was not allowed. " + date, LoggedInPerson?.UserName);
                return new RedirectToPageResult("/AccessDenied");
            }

            HasFinished = result.HasFinished;
            Person = SignInRepository.GetPerson(result.UserName) ?? Person.MrRobot;
            Score = result.Steps;
            PathTaken = result.PathTaken.Select(x => x.Replace("_", " ")).ToList();
            return Page();
        }
    }
}
