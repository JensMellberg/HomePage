using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class ChooseDayLogoModel(LogotypeRepository logotypeRepository,
        LogotypeResultRepository resultRepository,
        SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public required string Date { get; set; }

        public required string ReturnDate { get; set; }

        public required IEnumerable<LogotypeMetadata> AllLogos { get; set; }

        public IActionResult OnGet(string date)
        {
            Date = date;
            var convertedDate = DateHelper.FromKey(date);
            ReturnDate = DateHelper.ToKey(DateHelper.GetFirstOfWeek(convertedDate));
            AllLogos = logotypeRepository.LoadLogos();

            return Page();
        }

        public IActionResult OnPost(string date, string logoId)
        {
            var convertedDate = DateHelper.FromKey(date);
            if (!IsAdmin || !resultRepository.MayChangeDayLogo(convertedDate))
            {
                return BadRequest();
            }
            
            resultRepository.SetDayLogoMetadata(convertedDate, logoId);
            return new JsonResult(new
            {
                success = true
            });
        }
    }
}
