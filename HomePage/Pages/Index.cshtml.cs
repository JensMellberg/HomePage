using HomePage.Chores;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class PersonIcon
    {
        public string Id { get; set; }

        public bool WasDoneToday { get; set; }

        public int Streak { get; set; }

        public string Html {
            get {
                return Id switch
                {
                    "Litter" => "<div class=\"profile-photo cat\"></div>",
                    _ => $"<div class=\"profile-photo {Id} chore\" chore-value=\"{Id}\" was-done-today=\"{WasDoneToday}\" streak={Streak}></div>",
                };
            }
        }
    }

    [IgnoreAntiforgeryToken]
    public class IndexModel(AppDbContext dbContext, 
        RedDayRepository redDayRepository, 
        ThemeDayRepository themeDayRepository, 
        DayFoodRepository dayFoodRepository, 
        SignInRepository signInRepository, 
        SettingsRepository settingsRepository,
        ChoreRepository choreRepository) : BasePage(signInRepository)
    {

        public int Day { get; set; }

        public string Month { get; set; }

        public string DayOfWeek { get; set; }

        public string FixWeekQs { get; set; }

        public string AnniverseryString { get; set; }

        public DayFood? TodayFood { get; set; }

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

        public void OnGet()
        {
            var today = DateHelper.DateTimeNow;
            Day = today.Day;
            DayOfWeek = DateHelper.WeekNumberToString[(int)today.DayOfWeek];
            Month = DateHelper.MonthNumberToString[today.Month];

            Activities = dbContext.CalendarActivity.ToList().Where(x =>
            {
                var startDay = x.CalendarDate;
                if (x.IsReoccuring)
                {
                    startDay = startDay.AddYears(today.Year - startDay.Year);
                }

                var lastDay = startDay.AddDays(x.DurationInDays);
                return today > startDay && today < lastDay;
            });

            List<string> exemptFromChores = [];
            if (Activities.Any(x => x.IsVacation && (x.Person == "Both" || x.Person == Person.Jens.Name)))
            {
                exemptFromChores.Add(Person.Jens.Name);
            }

            if (Activities.Any(x => x.IsVacation && (x.Person == "Both" || x.Person == Person.Anna.Name)))
            {
                exemptFromChores.Add(Person.Anna.Name);
            }

            (Day % 2 == 0 ? AnnaIcons : JensIcons).Add(new PersonIcon { Id = "Litter" });
            var shouldSaveContext = choreRepository.GetAllChores().Select(AddChore).ToList().Any();
            if (shouldSaveContext)
            {
                dbContext.SaveChanges();
            }

            FixWeekQs = GetFirstOfWeekQueryString(today);
            if (Day == 16)
            {
                var togetherDate = new DateTime(2023, 1, 16);
                var yearsPassed = today.Year - togetherDate.Year;
                var monthsPassed = today.Month - togetherDate.Month;
                var monthString = monthsPassed != 0 ? $" och {monthsPassed} månader" : "";
                AnniverseryString = $"{yearsPassed} år{monthString}";
            }

            settingsRepository.PerformBackupAsync(false);
            var dayFoods = dayFoodRepository.GetPopulatedDayFood();
            var todaysFood = dayFoods.FirstOrDefault(x => x.Date == today.Date);
            TodayFood = todaysFood;

            var nextWeekDays = new List<string>();
            for (var i = 0; i < 7; i++)
            {
                var day = today.Date.AddDays(i);
                if (!dayFoods.Any(x => x.Date == day))
                {
                    nextWeekDays.Add(DateHelper.ToKey(day));
                }
            }

            NoFoodDaysList = string.Join(',', nextWeekDays);
            var imagePair = ImageRepository.Instance.GetImageUrl();
            MilliSecondsUntilNextPicture = ImageRepository.Instance.SecondsUntilNextUpdate * 1000;
            ImageUrl = imagePair.Item1;
            ImageTaken = imagePair.Item2;
            RedDay = redDayRepository.InfoForDate(today.Date);
            //new ThemeDayRepository().UpdateFromJsonFile();
            ThemeDays = themeDayRepository.InfoForDate(today.Date);

            bool AddChore(BaseChore chore)
            {
                var shouldSaveContext = chore.TryResetStreak(exemptFromChores);
                var wasDoneToday = chore.WasDoneToday();

                if (wasDoneToday != null)
                {
                    AddChoreIcon(chore, wasDoneToday, true);
                    return shouldSaveContext;
                }

                var chorePerson = chore.ChorePerson();
                if (chorePerson != null)
                {
                    AddChoreIcon(chore, chorePerson, false);
                }

                return shouldSaveContext;
            }

            void AddChoreIcon(BaseChore chore, string person, bool wasDoneToday)
            {
                var streak = chore.GetStreak(person).Streak;
                (person == Person.Anna.Name ? AnnaIcons : JensIcons).Add(new PersonIcon { Id = chore.Id, Streak = streak, WasDoneToday = wasDoneToday });
            }
        }

        private static string GetFirstOfWeekQueryString(DateTime date)
        {
            var firstDayOfWeek = DateHelper.GetFirstOfWeek(date);
            return DateHelper.FormatDateForQueryString(firstDayOfWeek);
        }

        public IActionResult OnPost(string date, Guid foodId)
        {
            if (foodId == Guid.Empty)
            {
                var allFoods = dbContext.Food.Where(x => !x.IsSideDish).ToArray();
                var randomizedFoodIndex = new Random().Next(allFoods.Length);
                var randomizedFood = allFoods[randomizedFoodIndex];
                return new JsonResult(new { id = randomizedFood.Id, food = randomizedFood.Name });
            }
            else
            {
                var redirectResult = GetPotentialClientRedirectResult(true, true);
                if (redirectResult != null)
                {
                    return redirectResult;
                }

                var idToUse = Guid.NewGuid();
                var foodConnection = new DayFoodDishConnection
                {
                    DayFoodId = idToUse,
                    FoodId = foodId,
                    IsMainDish = true
                };

                var dateTime = DateHelper.FromKey(date);
                var dayFood = new DayFood { Id = idToUse, Date = dateTime, Portions = 1, FoodConnections = [foodConnection] };
                dbContext.DayFood.Add(dayFood);
                dbContext.SaveChanges();
                return Utils.CreateRedirectClientResult("WeekFood?" + GetFirstOfWeekQueryString(dateTime));
            }
        }
    }
}
