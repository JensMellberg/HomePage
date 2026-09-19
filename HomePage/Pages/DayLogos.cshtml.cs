using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class DayLogosModel(LogotypeResultRepository resultRepository, LogotypeRepository logotypeRepository, SignInRepository signInRepository)
        : BasePage(signInRepository)
    {
        public List<(DateTime date, LogotypeMetadata? metadata)> DayLogos { get; set; } = [];

        public required string PreviousWeek { get; set; }

        public required string NextWeek { get; set; }

        public IActionResult OnGet(string fromDate)
        {
            var date = DateHelper.FromKey(fromDate);
            PreviousWeek = DateHelper.ToKey(date.AddDays(-7));
            NextWeek = DateHelper.ToKey(date.AddDays(7));
            for (var i = 0; i < 7; i++)
            {
                var crntDate = date.AddDays(i);
                var dayLogo = resultRepository.GetDayLogoMetadata(crntDate);
                DayLogos.Add((crntDate, dayLogo));
            }

            return Page();
        }

        public bool MayChangeDay(DateTime date) => resultRepository.MayChangeDayLogo(date);
    }
}
