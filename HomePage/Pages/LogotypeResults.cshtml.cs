using HomePage.Data;
using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{ 
    public class LogotypeResultsModel(LogotypeResultRepository resultRepository, SignInRepository signInRepository) 
        : BasePage(signInRepository)
    {
        public required List<LogotypeDayResult> PreviousResults { get; set; }

        public required Dictionary<string, Person> PersonByUserName{ get; set; }

        public LogotypeDayResult? TodaysResult { get; set; }

        public string ResultText(LogotypeResult result) =>
            $"{PersonByUserName[result.UserName].Name}: " +
            $"({result.HintsUsed} {(result.HintsUsed == 1 ? "ledtråd" : "ledtrådar")}," +
            $" {result.Guesses} {(result.Guesses == 1 ? "gissning" : "gissningar")})";

        public IActionResult OnGet()
        {
            var allDayResults = resultRepository.GetResultsByDay();
            PersonByUserName = SignInRepository.GetPersonByUserNames();
            var loggedInPerson = LoggedInPerson?.UserName;
            PreviousResults = allDayResults.previous;
            TodaysResult = allDayResults.current;
            return Page();
        }
    }
}
