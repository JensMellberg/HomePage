using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CreateDayFoodModel : PageModel
    {
        public string DateKey { get; set; }

        public string DateQs { get; set; }

        public string DateQsFirstOfWeek { get; set; }

        public string CurrentFilter { get; set; }

        public DayFood? DayFood { get; set; }

        public List<Food> ExistingFood { get; set; }

        public string FoodIdFromQueryString { get; set; }

        public bool CanDelete { get; set; }

        public IActionResult OnGet(int year, int month, int day, string foodId, string filter)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            var Regex = string.IsNullOrEmpty(filter) ? null : new Regex("^.*" + filter + ".*$", RegexOptions.IgnoreCase);
            var date = new DateTime(year, month, day);
            DateKey = DateHelper.ToKey(date);
            
            var existingDayFood = new DayFoodRepository().TryGetValue(DateKey);
            CanDelete = existingDayFood != null && CanBeDeleted(DateKey);
            DayFood = existingDayFood ?? new DayFood { Day = date };
            ExistingFood = new FoodRepository().GetValues().Values
                .Where(x => Regex?.IsMatch(x.Name) ?? true)
                .OrderBy(x => x.Name)
                .ToList();
            FoodIdFromQueryString = foodId;
            CurrentFilter = filter;
            DateQs = DateHelper.FormatDateForQueryString(date);
            DateQsFirstOfWeek = DateHelper.FormatDateForQueryString(DateHelper.GetFirstOfWeek(date));
            return Page();
        }

        public IActionResult OnPost(string day, string foodId, string delete)
        {
            var date = DateHelper.FromKey(day);
            var dayFood = new DayFood { Day = date, FoodId = foodId };

            if (!string.IsNullOrEmpty(delete) && this.CanBeDeleted(day))
            {
                new DayFoodRepository().Delete(dayFood.Key);
            } 
            else
            {
                new DayFoodRepository().SaveValue(dayFood);
            }
            
            return Redirect($"/WeekFood?{DateHelper.FormatDateForQueryString(DateHelper.GetFirstOfWeek(date))}");
        }

        private bool CanBeDeleted(string day) => !new FoodRankingRepository().GetValues().Values.Where(x => x.Day == day).Any();
    }
}
