using HomePage.Data;

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
                logger.Error("Tried to access word mix result that was not allowed. " + key, LoggedInPerson?.UserName);
                return;
            }

            PersonClass = result.Person == Person.Jens.Name || result.Person == Person.Anna.Name ? result.Person.ToLower() : "unknown";
            Score = result.Score;
            BoardString = result.Board.EncodeForClient();
        }
    }
}
