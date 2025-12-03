using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class CreateActivityModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public CalendarActivity Activity { get; set; }

        public bool IsNew { get; set; }

        public IActionResult OnGet(string activityId, string date)
        {
            var existingActivity = dbContext.CalendarActivity.Find(activityId);
            IsNew = string.IsNullOrEmpty(activityId);
            Activity = existingActivity ?? new CalendarActivity { CalendarDate = DateHelper.FromKey(date) };
            return Page();
        }

        public IActionResult OnPost(string activityId, string date, string text, string person, int duration, string isreoccuring, string isvacation, string delete)
        {
            var convertedDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(date));
            var existing = dbContext.CalendarActivity.Find(activityId);
            if (!string.IsNullOrEmpty(delete))
            {
                dbContext.CalendarActivity.Remove(existing);
            } else if (existing != null)
            {
                existing.Text = text;
                existing.Person = person;
                existing.CalendarDate = convertedDate;
                existing.DurationInDays = duration;
                existing.IsReoccuring = isreoccuring == "on";
                existing.IsVacation = isvacation == "on";
            } else
            {
                var activity = new CalendarActivity { Key = activityId, Text = text, Person = person, CalendarDate = convertedDate, DurationInDays = duration, IsReoccuring = isreoccuring == "on", IsVacation = isvacation == "on" };
                dbContext.CalendarActivity.Add(activity);
            }

            dbContext.SaveChanges();
            var firstOfWeekDate = DateHelper.GetFirstOfWeek(convertedDate);
            return Redirect($"/Calendar?{DateHelper.FormatDateForQueryString(firstOfWeekDate)}");
        }
    }
}
