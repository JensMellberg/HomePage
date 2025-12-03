using HomePage.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class WordMixStatModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public string PersonClass { get; set; }

        public int Score { get; set; }

        public string BoardString { get; set; }

        public void OnGet(Guid key)
        {
            var result = dbContext.WordMixResult.FirstOrDefault(x => x.Id == key);

            if (result == null || result.Date >= DateHelper.DateNow)
            {
                logger.Error("Tried to access word mix result that was not allowed. " + key, LoggedInPerson?.Name);
                return;
            }

            PersonClass = result.Person.ToLower();
            Score = result.Score;
            BoardString = result.Board.EncodeForClient();
        }
    }
}
