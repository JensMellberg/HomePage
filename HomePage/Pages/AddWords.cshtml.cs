using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using static HomePage.WordMixResultValidator;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireLogin]
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
                .Select(x => (x, x.Creator == LoggedInPerson?.UserName, CanApprove(x))).ToList();
           
            return Page();
            bool CanApprove(ExtraWord word) => !word.AnnaApproved && LoggedInPerson?.UserName == Person.Anna.Name || !word.JensApproved && LoggedInPerson?.Name == Person.Jens.Name;
        }

        public IActionResult OnPost(string action, string word)
        {
            var redirectResult = GetPotentialClientRedirectResult(false, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var loggedInName = LoggedInPerson!.UserName;
            if (action == "delete")
            {
                var existing = dbContext.ExtraWord.FirstOrDefault(x => x.Word == word);
                if (existing?.Creator == loggedInName)
                {
                    dbContext.ExtraWord.Remove(existing);
                    dbContext.SaveChanges();
                    return Utils.CreateClientResult(new { success = true });
                }

                return Utils.CreateAccessDeniedClientResult();
            }
            else if (action == "approve")
            {
                var existing = dbContext.ExtraWord.FirstOrDefault(x => x.Word == word);
                if (existing?.AnnaApproved == false && loggedInName == Person.Anna.Name)
                {
                    existing.AnnaApproved = true;
                }
                else if (existing?.JensApproved == false && loggedInName == Person.Jens.Name)
                {
                    existing.JensApproved = true;
                }
                else
                {
                    return Utils.CreateAccessDeniedClientResult();
                }

                dbContext.SaveChanges();
                return Utils.CreateClientResult(new { success = true });
            }
            else if (action == "new")
            {
                if (GetAllWords(dbContext).Contains(word.ToUpper()) || dbContext.ExtraWord.Any(x => x.Word == word.ToLower()))
                {
                    return Utils.CreateErrorClientResult("Ordet finns redan.");
                }

                var newWord = new ExtraWord { Word = word.ToLower(), Creator = loggedInName };
                if (loggedInName == Person.Jens.Name)
                {
                    newWord.JensApproved = true;
                }
                else if (loggedInName == Person.Anna.Name)
                {
                    newWord.AnnaApproved = true;
                }

                dbContext.ExtraWord.Add(newWord);
                dbContext.SaveChanges();
                return Utils.CreateClientResult(new { success = true });
            }

            return new BadRequestResult();
        }
    }
}
