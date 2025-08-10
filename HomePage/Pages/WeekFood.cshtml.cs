using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class WeekFoodModel : PageModel
    {
        public List<DayFood> DayFoods { get; set; } = new List<DayFood>();

        public Dictionary<string, FoodRanking[]> FoodRankings = new Dictionary<string, FoodRanking[]>();

        public string PrevWeekQs { get; set; }

        public string NextWeekQs { get; set; }

        public int Week { get; set; }

        public bool IsLoggedIn { get; set; }

        public string LoggedInPerson { get; set; }

        public bool IsInFuture(string date) => DateHelper.FromKey(date) > DateTime.Now;

        public List<WeekGoals> WeekGoals { get; set; } = [];

        public IActionResult OnGet(int year, int month, int day)
        {
            this.TryLogIn();
            IsLoggedIn = SignInRepository.IsLoggedIn(HttpContext.Session);
            LoggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name;
            var dayFoods = new DayFoodRepository().GetValues();
            var allFoods = new FoodRepository().GetValues();
            var allRankings = new FoodRankingRepository().GetValues();
            var categoriesWithGoal = new CategoryRepository().GetValues().Values.Where(x => x.HasGoal);
            var weekGoalsPerCategoryId = categoriesWithGoal.ToDictionary(x => x.Key, x => new WeekGoals
            {
                Category = x.Name,
                Goal = x.GoalPerWeek,
                IsBad = x.IsBad
            });

            var date = new DateTime(year, month, day);
            PrevWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(-7));
            NextWeekQs = DateHelper.FormatDateForQueryString(date.AddDays(7));

            if (date.DayOfWeek != DayOfWeek.Monday)
            {
                return RedirectToPage("Error");
            }

            var dfi = DateTimeFormatInfo.CurrentInfo;
            var cal = dfi.Calendar;

            Week = cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

            for (var i = 0; i < 7; i++)
            {
                var crntDate = date.AddDays(i);
                dayFoods.TryGetValue(DateHelper.ToKey(crntDate), out var dayFood);
                if (dayFood != null)
                {
                    dayFood.Food = allFoods[dayFood.FoodId];
                    foreach (var category in dayFood.Food.CategoriyIds.Where(weekGoalsPerCategoryId.ContainsKey))
                    {
                        weekGoalsPerCategoryId[category].Completed++;
                    }
                }
                
                DayFoods.Add(dayFood ?? new DayFood { FoodId = null, Day = crntDate });
                allRankings.TryGetValue(FoodRanking.MakeId(crntDate, Person.Anna.Name), out var ranking1);
                allRankings.TryGetValue(FoodRanking.MakeId(crntDate, Person.Jens.Name), out var ranking2);
                ranking1 = ranking1 ?? new FoodRanking { Day = DateHelper.ToKey(crntDate), Person = Person.Anna.Name };
                ranking2 = ranking2 ?? new FoodRanking { Day = DateHelper.ToKey(crntDate), Person = Person.Jens.Name };
                FoodRankings.Add(DateHelper.ToKey(crntDate), [ranking1, ranking2]);
            }

            WeekGoals = weekGoalsPerCategoryId.Values.ToList();
            return Page();
        }

        public IActionResult OnPost(string date, string person, int ranking)
        {
            if (!SignInRepository.IsLoggedIn(HttpContext.Session) || person != SignInRepository.LoggedInPerson(HttpContext.Session).Name)
            {
                return new OkResult();
            }

            var foodId = new DayFoodRepository().TryGetValue(date).FoodId;
            var foodRanking = new FoodRanking { Day = date, Person = person, Ranking = ranking, FoodId = foodId };
            new FoodRankingRepository().SaveValue(foodRanking);
            return new OkResult();
        }
    }

    public class WeekGoals
    {
        public string Category { get; set; }

        public int Goal { get; set; }

        public int Completed { get; set; }

        public bool IsBad { get; set; }

        public string Style
        {
            get
            {
                if (IsBad)
                {
                    return Goal < Completed ? "color: red;" : "color: green;";
                }

                return Goal <= Completed ? "color: green;" : "color: red;";
            }
        }
    }
}
