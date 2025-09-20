using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CurrentWordMixModel : PageModel
    {
        public string BoardString { get; set; }

        public string AvailableLetters { get; set; }

        public string LoggedInPerson { get; set; }

        public int CurrentBest { get; set; }

        public string ExtraWords { get; set; }

        public string ConvertScore(int score) => score == 0 ? "-" : score.ToString();

        public IActionResult OnGet(string board)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            if (!string.IsNullOrEmpty(board))
            {
                TempData["Board"] = board;
                return new RedirectResult("/CurrentWordMix");
            }

            BoardString = TempData["Board"] as string ?? "";
            LoggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name!;
            AvailableLetters = new CurrentWordMixRepository().GetCurrent().Letters.EncodeForClient();
            CurrentBest = new WordMixResultRepository().TryGetValue(WordMixResult.MakeId(DateHelper.DateNow, LoggedInPerson))?.Score ?? 0;
            ExtraWords = string.Join(",", new ExtraWordRepository().GetValues().Values.Select(x => x.Word.ToUpper().EncodeForClient()));
            return Page();
        }

        public IActionResult OnPost(int score, string letters, bool ping)
        {
            if (ping)
            {
                return new JsonResult(new { success = true });
            }

            var loggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name;
            if (string.IsNullOrEmpty(loggedInPerson))
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrEmpty(letters))
            {
                new WordMixResultRepository().UpdateResultWithValidation(loggedInPerson, letters, score);
            }

            return new JsonResult(new { success = true });
        }
    }
}
