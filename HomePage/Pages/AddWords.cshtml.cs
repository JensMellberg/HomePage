using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using static HomePage.WordMixResultValidator;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class AddWordsModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<(ExtraWord model, bool canDelete, bool canApprove)> AllWords { get; set; }

        public string VisibilityText(bool canDelete) => canDelete ? "visible" : "hidden";

        public IActionResult OnGet()
        {
            AllWords = dbContext.ExtraWord
                .OrderBy(x => x.AnnaApproved && x.JensApproved ? 1 : 0)
                .ThenBy(x => x.Word)
                .ToList()
                .Select(x => (x, x.Creator == LoggedInPerson?.Name, CanApprove(x))).ToList();
           
            return Page();
            bool CanApprove(ExtraWord word) => !word.AnnaApproved && LoggedInPerson?.Name == Person.Anna.Name || !word.JensApproved && LoggedInPerson?.Name == Person.Jens.Name;
        }

        public IActionResult OnPost(string action, string word)
        {
            if (string.IsNullOrEmpty(LoggedInPerson?.Name))
            {
                return new BadRequestResult();
            }

            if (action == "delete")
            {
                var existing = dbContext.ExtraWord.FirstOrDefault(x => x.Word == word);
                if (existing?.Creator == LoggedInPerson.Name)
                {
                    dbContext.ExtraWord.Remove(existing);
                    dbContext.SaveChanges();
                }

                return new JsonResult(new { success = true });
            }
            else if (action == "approve")
            {
                var existing = dbContext.ExtraWord.FirstOrDefault(x => x.Word == word);
                if (existing?.AnnaApproved == false && LoggedInPerson.Name == Person.Anna.Name)
                {
                    existing.AnnaApproved = true;
                }
                else if (existing?.JensApproved == false && LoggedInPerson.Name == Person.Jens.Name)
                {
                    existing.JensApproved = true;
                }
                else
                {
                    return new JsonResult(new { success = false });
                }

                dbContext.SaveChanges();
                return new JsonResult(new { success = true });
            }
            else if (action == "new")
            {
                if (GetAllWords(dbContext).Contains(word.ToUpper()) || dbContext.ExtraWord.Any(x => x.Word == word.ToLower()))
                {
                    return new JsonResult(new { success = false });
                }

                var newWord = new ExtraWord { Word = word.ToLower(), Creator = LoggedInPerson.Name };
                if (LoggedInPerson.Name == Person.Jens.Name)
                {
                    newWord.JensApproved = true;
                }
                else if (LoggedInPerson.Name == Person.Anna.Name)
                {
                    newWord.AnnaApproved = true;
                }

                dbContext.ExtraWord.Add(newWord);
                dbContext.SaveChanges();
                return new JsonResult(new { success = true });
            }

            return new BadRequestResult();
        }
    }
}
