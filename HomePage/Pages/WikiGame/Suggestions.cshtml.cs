using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages.WikiGame
{
    [IgnoreAntiforgeryToken]
    public class SuggestionsModel(SignInRepository signInRepository, WikiGameRepository wikiGameRepository, AppDbContext dbContext) : BasePage(signInRepository)
    {
        public List<WikiGameSuggestion> Suggestions { get; set; }
        public IActionResult OnGet()
        {
            Suggestions = wikiGameRepository.GetSuggestions();
            return Page();
        }

        public IActionResult OnPost(Guid id, bool isVote)
        {
            var suggestion = dbContext.WikiGameSuggestions.Find(id);
            if (suggestion == null || LoggedInPerson == null)
            {
                return Utils.CreateAccessDeniedClientResult();
            }

            if (isVote)
            {
                if (suggestion.Voters.Contains(LoggedInPerson!.UserName))
                {
                    suggestion.Votes--;
                    suggestion.Voters.Remove(LoggedInPerson!.UserName);
                }
                else
                {
                    suggestion.Votes++;
                    suggestion.Voters.Add(LoggedInPerson!.UserName);
                }
            } else
            {
                if (!(IsAdmin || suggestion.Creator == LoggedInPerson?.UserName))
                {
                    return Utils.CreateAccessDeniedClientResult();
                }

                dbContext.WikiGameSuggestions.Remove(suggestion);
            }
            
            dbContext.SaveChanges();
            return Utils.CreateClientResult(null);
        }
    }
}
