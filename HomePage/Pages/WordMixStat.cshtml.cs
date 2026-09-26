using HomePage.Data;

namespace HomePage.Pages
{
    public class WordMixStatModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public required Person Person { get; set; }

        public int Score { get; set; }

        public required string BoardString { get; set; }

        public void OnGet(Guid key)
        {
            var result = dbContext.WordMixResult.FirstOrDefault(x => x.Id == key);

            if (result == null || result.Date >= DateHelper.DateNow)
            {
                logger.Error("Tried to access word mix result that was not allowed. " + key, LoggedInPerson?.UserName);
                return;
            }

            Person = SignInRepository.GetPerson(result.Person)!;
            Score = result.Score;
            BoardString = result.Board.EncodeForClient();
        }
    }
}
