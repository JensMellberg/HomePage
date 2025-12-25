using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using static HomePage.WordMixResultValidator;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class RunRobotModel(AppDbContext dbContext, 
        SignInRepository signInRepository, 
        CurrentWordMixRepository currentWordMixRepository,
        DatabaseLogger logger) : BasePage(signInRepository)
    {
        public IActionResult OnGet(string date)
        {
            if (date != null)
            {
                var dateTime = date == "today" ? DateHelper.DateNow : DateHelper.FromKey(date);
                var anyResult = dbContext.WordMixResult.FirstOrDefault(x => x.Date == dateTime);
                CurrentWordMix currentWordMix;
                if (anyResult != null)
                {
                    var board = Board.ParseFromString(anyResult.Board);
                    currentWordMix = new CurrentWordMix { Letters = string.Join("", board.AllLetters.Select(x => x.Character.ToString() + x.Score)) };
                } else if (dateTime == DateHelper.DateNow)
                {
                    currentWordMix = currentWordMixRepository.GetCurrent();
                } else
                {
                    logger.Error($"Cannot run robot for a date without any result.", LoggedInPerson?.UserName);
                    return new RedirectToPageResult("/Error");
                }

                currentWordMixRepository.AddRobotResult(dateTime, currentWordMix);
            }

            var referer = Request.Headers.Referer.ToString();

            if (!string.IsNullOrWhiteSpace(referer))
            {
                return Redirect(referer);
            }

            return RedirectToPage("/Index");
        }
    }
}
