using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireLogin]
    public class CreateLogotypeModel(LogotypeRepository logotypeRepository, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public void OnGet()
        {
        }

        [BindProperty]
        public required string Answer { get; set; }

        [BindProperty]
        public int Hints { get; set; }

        public IActionResult OnPost(IFormFile logo, IFormFile? originalLogo)
        {
            var logoValidationError = Utils.ValidateImage(logo);
            if (logoValidationError != null)
            {
                ModelState.AddModelError(nameof(logo), logoValidationError);
                return Page();
            }

            if (originalLogo != null)
            {
                var originalLogoValidationError = Utils.ValidateImage(originalLogo);
                if (originalLogoValidationError != null)
                {
                    ModelState.AddModelError(nameof(originalLogo), originalLogoValidationError);
                    return Page();
                }
            }

            if (string.IsNullOrEmpty(Answer) || Answer.Length < 2)
            {
                ModelState.AddModelError(nameof(Answer), "Det måste finnas ett rätt svar med minst 2 tecken.");
                return Page();
            }

            if (Hints == 0 || Hints > Answer.Length)
            {
                ModelState.AddModelError(nameof(Hints), "Antal ledtrådar måste vara mellan 0 och svarets längd.");
                return Page();
            }

            logotypeRepository.SaveLogo([Answer], LoggedInPerson!.UserName, Hints, logo, originalLogo);
            logger.Information($"{LoggedInPerson!.UserName} added a logo with answer {Answer}");
            return RedirectToPage("ListLogos");
        }
    }
}
