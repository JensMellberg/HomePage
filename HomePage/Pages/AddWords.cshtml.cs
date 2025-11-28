using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static HomePage.WordMixResultValidator;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class AddWordsModel : PageModel
    {
        public List<(ExtraWord model, bool canDelete, bool canApprove)> AllWords { get; set; }

        public string VisibilityText(bool canDelete) => canDelete ? "visible" : "hidden";

        public string LoggedInPerson { get; set; }

        public IActionResult OnGet()
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            LoggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name!;
            AllWords = new ExtraWordRepository().GetValues().Values
                .OrderBy(x => x.AnnaApproved && x.JensApproved ? 1 : 0)
                .ThenBy(x => x.Word)
                .Select(x => (x, x.Creator == LoggedInPerson, CanApprove(x))).ToList();
           
            return Page();
            bool CanApprove(ExtraWord word) => !word.AnnaApproved && LoggedInPerson == Person.Anna.Name || !word.JensApproved && LoggedInPerson == Person.Jens.Name;
        }

        public IActionResult OnPost(string action, string word)
        {
            var repo = new ExtraWordRepository();
            var loggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name;
            if (string.IsNullOrEmpty(loggedInPerson))
            {
                return new BadRequestResult();
            }

            if (action == "delete")
            {
                var existing = repo.TryGetValue(word);
                if (existing?.Creator == loggedInPerson)
                {
                    repo.Delete(word);
                }

                return new JsonResult(new { success = true });
            }
            else if (action == "approve")
            {
                var existing = repo.TryGetValue(word);
                if (existing?.AnnaApproved == false && loggedInPerson == Person.Anna.Name)
                {
                    existing.AnnaApproved = true;
                }
                else if (existing?.JensApproved == false && loggedInPerson == Person.Jens.Name)
                {
                    existing.JensApproved = true;
                }
                else
                {
                    return new JsonResult(new { success = false });
                }

                repo.SaveValue(existing);
                return new JsonResult(new { success = true });
            }
            else if (action == "new")
            {
                if (GetAllWords().Contains(word.ToUpper()) || repo.TryGetValue(word.ToLower()) != null)
                {
                    return new JsonResult(new { success = false });
                }

                var newWord = new ExtraWord { Key = word.ToLower(), Creator = loggedInPerson };
                if (loggedInPerson == Person.Jens.Name)
                {
                    newWord.JensApproved = true;
                }
                else if (loggedInPerson == Person.Anna.Name)
                {
                    newWord.AnnaApproved = true;
                }

                repo.SaveValue(newWord);
                return new JsonResult(new { success = true });
            }

            return new BadRequestResult();
        }
    }
}
