using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class WordMixStatModel : PageModel
    {
        public string PersonClass { get; set; }

        public int Score { get; set; }

        public string BoardString { get; set; }

        public void OnGet(string key)
        {
            var result = new WordMixResultRepository()
                .TryGetValue(key);

            if (result == null || DateHelper.FromKey(result.Day).Date >= DateHelper.DateNow.Date)
            {
                throw new Exception("Tried to access word mix result that was not allowed. " + key);
            }

            PersonClass = result.Person.ToLower();
            Score = result.Score;
            BoardString = result.Board.EncodeForClient();
        }
    }
}
