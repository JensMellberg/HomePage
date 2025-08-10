using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HomePage.Pages
{
    public class CalendarModel : PageModel
    {
        public string PrevWeekQs { get; set; }

        public string NextWeekQs { get; set; }

        public List<CalendarDayModel> DayModels { get; set; } = [];

        public IActionResult OnGet(int year, int month, int day)
        {
            this.TryLogIn();
            var date = new DateTime(year, month, day);
            PrevWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(-7));
            NextWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(7));
            if (date.DayOfWeek != DayOfWeek.Monday)
            {
                return RedirectToPage("Error");
            }

            var calendarAcitivities = new CalendarActivityRepository().GetValues().Values;
            for (int i = 0; i < 7; i++)
            {
                var crntDate = date.AddDays(i);
                var dateAsKey = DateHelper.ToKey(crntDate);
                var activities = calendarAcitivities.Where(x => x.Date == dateAsKey || IsDifferentYear(x.Date, dateAsKey) && x.IsReoccuring).ToList();
                if (i == 0)
                {
                    foreach (var possibleOverlap in calendarAcitivities)
                    {
                        var dateTime = DateHelper.FromKey(possibleOverlap.Date);
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

            bool IsDifferentYear(string date1, string date2) => date1.Substring(4) == date2.Substring(4);
        }
    }

    public class CalendarDayModel
    {
        public string DayText { get; set; }

        public DateTime Day { get; set; }

        public IEnumerable<CalendarActivity> Activities { get; set; }
    }
}
