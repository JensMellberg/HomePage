using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class DayLogosModel(LogotypeResultRepository resultRepository, SignInRepository signInRepository)
        : BasePage(signInRepository)
    {
        public List<(DateTime date, LogotypeMetadata? metadata, bool isHidden)> DayLogos { get; set; } = [];

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
                DayLogos.Add((crntDate, dayLogo, dayLogo?.GetCreator != LoggedInPerson!.UserName));
            }

            return Page();
        }

        public bool MayChangeDay(DateTime date) => resultRepository.MayChangeDayLogo(date, LoggedInPerson!.UserName);
    }
}
