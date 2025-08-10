using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CreateActivityModel : PageModel
    {
        public CalendarActivity Activity { get; set; }

        public bool IsNew { get; set; }

        public IActionResult OnGet(string activityId, string date)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            var existingActivity = new CalendarActivityRepository().TryGetValue(activityId ?? string.Empty);
            IsNew = string.IsNullOrEmpty(activityId);
            Activity = existingActivity ?? new CalendarActivity { Date = date };
            Activity.Date = DateHelper.KeyWithZerosFromKey(Activity.Date);
            return Page();
        }

        public IActionResult OnPost(string activityId, string date, string text, string person, int duration, string isreoccuring, string delete)
        {
            var keyWithoutZeros = DateHelper.KeyFromKeyWithZeros(date);
            if (!string.IsNullOrEmpty(delete))
            {
                new CalendarActivityRepository().Delete(activityId);
            } else
            {
                var activity = new CalendarActivity { Key = activityId, Text = text, Person = person, Date = keyWithoutZeros, DurationInDays = duration, IsReoccuring = isreoccuring == "on" };

                new CalendarActivityRepository().SaveValue(activity);
                
            }

            var firstOfWeekDate = DateHelper.GetFirstOfWeek(DateHelper.FromKey(keyWithoutZeros));
            return Redirect($"/Calendar?{DateHelper.FormatDateForQueryString(firstOfWeekDate)}");
        }
    }
}
