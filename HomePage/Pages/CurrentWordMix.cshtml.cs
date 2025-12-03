using System.Text;
using HomePage.Data;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CurrentWordMixModel(CurrentWordMixRepository currentWordMixRepository, 
        WordMixResultRepository wordMixResultRepository,
        AppDbContext dbContext, 
        SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public string BoardString { get; set; }

        public string AvailableLetters { get; set; }

        public int CurrentBest { get; set; }

        public string ExtraWords { get; set; }

        public string ConvertScore(int score) => score == 0 ? "-" : score.ToString();

        public IActionResult OnGet(string board)
        {
            if (!string.IsNullOrEmpty(board))
            {
                TempData["Board"] = board;
                return new RedirectResult("/CurrentWordMix");
            }

            BoardString = TempData["Board"] as string ?? "";
            AvailableLetters = currentWordMixRepository.GetCurrent().Letters.EncodeForClient();
            CurrentBest = wordMixResultRepository.GetForDateAndPerson(DateHelper.DateNow, LoggedInPerson!.Name)?.Score ?? 0;
            ExtraWords = string.Join(",", dbContext.ExtraWord.Select(x => x.Word.ToUpper().EncodeForClient()));
            return Page();
        }

        public IActionResult OnPost(int score, string letters, bool ping)
        {
            if (ping)
            {
                return new JsonResult(new { success = true });
            }

            var loggedInPerson = LoggedInPerson?.Name;
            if (string.IsNullOrEmpty(loggedInPerson))
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrEmpty(letters))
            {
                wordMixResultRepository.UpdateResultWithValidation(loggedInPerson, letters, score);
            }

            return new JsonResult(new { success = true });
        }
    }
}
