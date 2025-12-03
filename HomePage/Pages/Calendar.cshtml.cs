using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CalendarModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public string PrevWeekQs { get; set; }

        public string NextWeekQs { get; set; }

        public List<CalendarDayModel> DayModels { get; set; } = [];

        public IActionResult OnGet(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            PrevWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(-7));
            NextWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(7));
            if (date.DayOfWeek != DayOfWeek.Monday)
            {
                return RedirectToPage("Error");
            }

            var calendarAcitivities = dbContext.CalendarActivity.ToList();
            for (int i = 0; i < 7; i++)
            {
                var crntDate = date.AddDays(i);
                var activities = calendarAcitivities.Where(x => x.CalendarDate == crntDate || IsDifferentYear(x.CalendarDate, crntDate) && x.IsReoccuring).ToList();
                if (i == 0)
                {
                    foreach (var possibleOverlap in calendarAcitivities)
                    {
                        var dateTime = possibleOverlap.CalendarDate;
                        if (dateTime < date && dateTime.AddDays(possibleOverlap.DurationInDays) > date)
                        {
                            possibleOverlap.DurationInDays -= (date - dateTime).Days;
                            possibleOverlap.DateInCalendar = date;
                            activities.Add(possibleOverlap);
                        }
                    }
                }

                DayModels.Add(new CalendarDayModel {
                    DayText = DateHelper.WeekNumberToString[(int)crntDate.DayOfWeek] + " " + crntDate.Day + "/" + crntDate.Month,
                    Activities = activities.OrderByDescending(x => x.DurationInDays),
                    Day = crntDate
                });
            }

            return Page();

            bool IsDifferentYear(DateTime date1, DateTime date2) => date1.Month == date2.Month && date1.Day == date2.Day;
        }
    }

    public class CalendarDayModel
    {
        public string DayText { get; set; }

        public DateTime Day { get; set; }

        public IEnumerable<CalendarActivity> Activities { get; set; }
    }
}
