using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class PersonIcon
    {
        public string Id { get; set; }

        public bool WasDoneToday { get; set; }

        public string Html {
            get {
                return Id switch
                {
                    "Litter" => "<div class=\"profile-photo cat\"></div>",
                    _ => $"<div class=\"profile-photo {Id} chore\" chore-value=\"{Id}\" was-done-today=\"{WasDoneToday}\"></div>",
                };
            }
        }
    }

    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public int Day { get; set; }

        public string Month { get; set; }

        public string DayOfWeek { get; set; }

        public string FixWeekQs { get; set; }

        public string AnniverseryString { get; set; }

        public Food? TodayFood { get; set; }

        public List<PersonIcon> JensIcons { get; set; } = [];

        public List<PersonIcon> AnnaIcons { get; set; } = [];

        public string NoFoodDaysList { get; set; }

        public string ImageUrl { get; set; }

        public string ImageTaken { get; set; }

        public RedDay? RedDay { get; set; }

        public IEnumerable<ThemeDay> ThemeDays { get; set; }

        public string WrapperClass => RedDay?.IsRed == true ? "red-day" : "";

        public IEnumerable<CalendarActivity> Activities { get; set; }

        public int MilliSecondsUntilNextPicture { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            this.TryLogIn();
            var today = DateTime.Now;
            var dummyTimeSpan = today - new DateTime(2020, 10, 15);
            Day = today.Day;
            DayOfWeek = DateHelper.WeekNumberToString[(int)today.DayOfWeek];
            Month = DateHelper.MonthNumberToString[today.Month];
            (Day % 2 == 0 ? AnnaIcons : JensIcons).Add(new PersonIcon { Id = "Litter" });
            AddChore(SettingsRepository.FlossChore);
            AddChore(SettingsRepository.FlowerChore);
            AddChore(SettingsRepository.BedSheetChore);
            AddChore(SettingsRepository.WorkoutChore);
            AddChore(SettingsRepository.EyeChore);
            AddChore(SettingsRepository.SinkChore);

            FixWeekQs = GetFirstOfWeekQueryString(today);
            if (Day == 16)
            {
                var togetherDate = new DateTime(2023, 1, 16);
                var yearsPassed = today.Year - togetherDate.Year;
                var monthsPassed = today.Month - togetherDate.Month;
                var monthString = monthsPassed != 0 ? $" och {monthsPassed} månader" : "";
                AnniverseryString = $"{yearsPassed} år{monthString}";
            }

            new SettingsRepository().PerformBackupAsync(false);
            var dayFoods = new DayFoodRepository().GetValues();
            dayFoods.TryGetValue(DateHelper.ToKey(today), out var todaysFood);
            if (!string.IsNullOrEmpty(todaysFood?.FoodId))
            {
                TodayFood = new FoodRepository().TryGetValue(todaysFood.FoodId);
            }

            var nextWeekDays = new List<string>();
            for (var i = 0; i < 7; i++)
            {
                var dayKey = DateHelper.ToKey(today.AddDays(i));
                if (!dayFoods.ContainsKey(dayKey))
                {
                    nextWeekDays.Add(dayKey);
                }
            }

            NoFoodDaysList = string.Join(',', nextWeekDays);
            var imagePair = ImageRepository.Instance.GetImageUrl();
            MilliSecondsUntilNextPicture = ImageRepository.Instance.SecondsUntilNextUpdate * 1000;
            ImageUrl = imagePair.Item1;
            ImageTaken = imagePair.Item2;
            RedDay = new RedDayRepository().InfoForDate(today);
            //new ThemeDayRepository().UpdateFromJsonFile();
            ThemeDays = new ThemeDayRepository().InfoForDate(today);
            Activities = new CalendarActivityRepository().GetValues().Values.Where(x =>
            {
                var startDay = DateHelper.FromKey(x.Date);
                if (x.IsReoccuring)
                {
                    startDay = startDay.AddYears(today.Year - startDay.Year);
                }

                var lastDay = startDay.AddDays(x.DurationInDays);
                return today > startDay && today < lastDay;
            });

            void AddChore(PersonChore chore)
            {
                var wasDoneToday = chore.WasDoneToday();
                if (wasDoneToday != null)
                {
                    (wasDoneToday == Person.Anna.Name ? AnnaIcons : JensIcons).Add(new PersonIcon { Id = chore.Id, WasDoneToday = true});
                    return;
                }

                var chorePerson = chore.ChorePerson();
                if (chorePerson != null)
                {
                    (chorePerson == Person.Anna.Name ? AnnaIcons : JensIcons).Add(new PersonIcon { Id = chore.Id });
                }
            }
        }

        private static string GetFirstOfWeekQueryString(DateTime date)
        {
            var firstDayOfWeek = DateHelper.GetFirstOfWeek(date);
            return DateHelper.FormatDateForQueryString(firstDayOfWeek);
        }

        public IActionResult OnPost(string date, string foodId)
        {
            if (string.IsNullOrEmpty(foodId))
            {
                var allFoods = new FoodRepository().GetValues().Values.ToArray();
                var randomizedFoodIndex = new Random().Next(allFoods.Length);
                var randomizedFood = allFoods[randomizedFoodIndex];
                return new JsonResult(new { id = randomizedFood.Key, food = randomizedFood.Name });
            }
            else
            {
                if (this.ShouldRedirectToLogin())
                {
                    return new JsonResult(new { redirectUrl = "Login" });
                }

                var dateTime = DateHelper.FromKey(date);
                var dayFood = new DayFood() { Day = dateTime, FoodId = foodId };
                new DayFoodRepository().SaveValue(dayFood);
                return new JsonResult(new { redirectUrl = "WeekFood?" + GetFirstOfWeekQueryString(dateTime) });
            }
        }
    }
}
