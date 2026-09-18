using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireLogin]
    public class CurrentLogotypeModel(LogotypeResultRepository resultRepository, LogotypeRepository logotypeRepository, SignInRepository signInRepository)
        : BasePage(signInRepository)
    {
        public required LogotypeResult Result { get; set; }

        public required string LogoUrl { get; set; }

        public string? CorrectAnswer { get; set; } = null;

        public int MaxHints { get; set; }

        public required string LetterHints { get; set; }

        public IActionResult OnGet()
        {
            var logoMetadata = resultRepository.GetDayLogoMetadata(DateHelper.DateNow);
            if (logoMetadata == null)
            {
                return NotFound();
            }

            Result = resultRepository.GetResultForPerson(DateHelper.DateNow, LoggedInPerson!.UserName);
            LogoUrl = Result.IsCorrect ? logoMetadata.GetOriginalLogo : logoMetadata.LogoUrl;
            MaxHints = logoMetadata.HintsAvailable;
            LetterHints = logoMetadata.Answers.First()[..Result.HintsUsed];
            
            if (Result.IsCorrect)
            {
                CorrectAnswer = logoMetadata.Answers.First();
            }

            return Page();
        }

        public IActionResult OnPost(string? answer, bool useHint)
        {
            if (!string.IsNullOrEmpty(answer))
            {
                return PerformGuess(answer);
            } 
            else if (useHint)
            {
                return UseHint();
            }

            return new JsonResult(new
            {
                isCorrect = false
            });
        }

        private JsonResult UseHint()
        {
            var lettersToShow = resultRepository.UseHint(LoggedInPerson!.UserName, DateHelper.DateNow);
            if (lettersToShow == null)
            {
                return new JsonResult(new
                {
                    isSuccess = false
                });
            }

            return new JsonResult(new
            {
                lettersToShow
            });
        }

        private JsonResult PerformGuess(string answer)
        {
            var isCorrect = resultRepository.PerformGuess(LoggedInPerson!.UserName, answer, DateHelper.DateNow);
            if (isCorrect)
            {
                var metadata = resultRepository.GetDayLogoMetadata(DateHelper.DateNow)!;

                return new JsonResult(new
                {
                    isCorrect,
                    correctAnswer = metadata.Answers.First(),
                    url = metadata.GetOriginalLogo
                });
            }

            return new JsonResult(new
            {
                isCorrect = false
            });
        }
    }
}
