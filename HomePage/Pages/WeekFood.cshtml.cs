using System.Globalization;
using HomePage.Achievements;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class WeekFoodModel(FoodStorageRepository foodStorageRepository, DayFoodRepository dayFoodRepository,
        AppDbContext dbContext, DatabaseLogger logger,
        AchievementRepository achievementRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<DayFood> DayFoods { get; set; } = new List<DayFood>();

        public Dictionary<string, FoodRanking[]> FoodRankings = new Dictionary<string, FoodRanking[]>();

        public string PrevWeekQs { get; set; }

        public string NextWeekQs { get; set; }

        public int Week { get; set; }

        public bool IsInFuture(DateTime date) => date > DateTime.Now;

        public List<WeekGoals> WeekGoals { get; set; } = [];

        public string ShopListFrom { get; set; }

        public string ShopListTo { get; set; }

        public Dictionary<string, string> RankingCommentsPerDay { get; set; }

        public IActionResult OnGet(int year, int month, int day)
        {
            var dayFoods = dayFoodRepository.GetPopulatedDayFood();
            var allRankings = dbContext.FoodRanking.ToList();
            var categoriesWithGoal = dbContext.Category.ToList().Where(x => x.HasGoal).ToList();
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
                var dayFood = dayFoods.FirstOrDefault(x => x.Date == crntDate);
                if (dayFood != null)
                {
                    foreach (var category in dayFood!.GetCategories(dbContext).Select(x => x.Key).Where(weekGoalsPerCategoryId.ContainsKey))
                    {
                        weekGoalsPerCategoryId[category].Completed++;
                    }
                }

                DayFoods.Add(dayFood ?? new EmptyDayFood { Date = crntDate });
                var ranking1 = allRankings.FirstOrDefault(x => x.Date == crntDate && x.Person == Person.Anna.Name);
                var ranking2 = allRankings.FirstOrDefault(x => x.Date == crntDate && x.Person == Person.Jens.Name);
                ranking1 = ranking1 ?? new FoodRanking { Date = crntDate, Person = Person.Anna.Name };
                ranking2 = ranking2 ?? new FoodRanking { Date = crntDate, Person = Person.Jens.Name };
                FoodRankings.Add(DateHelper.ToKey(crntDate), [ranking1, ranking2]);
            }

            WeekGoals = weekGoalsPerCategoryId.Values.ToList();

            var today = DateHelper.DateNow;

            var shopListFrom = today > date && (today - date).Days < 7 ? today : date;
            ShopListFrom = DateHelper.KeyWithZerosFromKey(DateHelper.ToKey(shopListFrom));
            ShopListTo = DateHelper.KeyWithZerosFromKey(DateHelper.ToKey(shopListFrom.AddDays(7)));

            return Page();
        }

        public IActionResult OnPost(string date, string person, int ranking, string note)
        {
            if (!IsAdmin || person != LoggedInPerson?.UserName)
            {
                return new OkResult();
            }

            var convertedDate = DateHelper.FromKey(date);
            var allDayFood = dayFoodRepository.GetPopulatedDayFood();
            var dayFood = allDayFood
                .FirstOrDefault(x => x.Date == convertedDate);

            if (dayFood == null)
            {
                return BadRequest();
            }

            var existsOnDay = dbContext.FoodRanking.Any(x => x.Date == convertedDate);
            if (!existsOnDay)
            {
                var ingredientsLost = dayFood.MainFood.FoodIngredients.Select(x => x.ToIngredientInstance()).ToList();
                foreach (var ingrs in dayFood.SideDishes.SelectMany(x => x.FoodIngredients.Select(x => x.ToIngredientInstance())))
                {
                    ingredientsLost.Add(ingrs);
                }

                var storageItems = foodStorageRepository.GetIngredients().ToDictionary(x => x.IngredientId, x => x);
                foreach (var ingr in ingredientsLost)
                {
                    if (storageItems.TryGetValue(ingr.IngredientId, out var storageItem))
                    {
                        ingr.MultiplyAmount(dayFood.Portions);
                        storageItem.Subtract(ingr);
                        logger.Information($"Ingredient {ingr.Ingredient.Name} was automatically reduced by {ingr.GetStaticDisplayString()} after eating food {dayFood.CombinedName}", person);
                    }
                }

                var itemsToStore = storageItems.Values.Where(x => x.IsNonZero).Select(x => new FoodStorageItem
                {
                    IngredientId = x.IngredientId ?? Guid.Empty,
                    Amount = x.Amount.Amount
                });

                dbContext.FoodStorage.RemoveRange(dbContext.FoodStorage);
                dbContext.FoodStorage.AddRange(itemsToStore);
                dbContext.SaveChanges();
            }

            var existing = dbContext.FoodRanking.FirstOrDefault(x => x.Date == convertedDate && x.Person == person);
            if (existing != null)
            {
                existing.Ranking = ranking;
                existing.Note = note;
                existing.FoodId = dayFood!.MainFoodId.ToString();
            } else
            {
                var foodRanking = new FoodRanking { Date = convertedDate, Person = person, Ranking = ranking, FoodId = dayFood!.MainFoodId.ToString(), Note = note };
                dbContext.FoodRanking.Add(foodRanking);
            }

            logger.Information($"{person} gave food {dayFood.CombinedName} a score of {ranking}", person);
            dbContext.SaveChanges();

            if (dayFood.GetCategories(dbContext).Any(x => x.Name == "Vegetariskt"))
            {
                var firstOfWeek = DateHelper.GetFirstOfWeek(convertedDate);
                var totalVego = 0;
                for (var weekDelta = -1; weekDelta > -10; weekDelta--)
                {
                    var weekDate = firstOfWeek.AddDays(7 * weekDelta);
                    var hasVego = false;
                    for (var i = 0; i < 7; i++)
                    {
                        var crnt = weekDate.AddDays(i);
                        var dayFoodOfDay = allDayFood
                            .FirstOrDefault(x => x.Date == crnt);
                        if (dayFoodOfDay == null)
                        {
                            continue;
                        }

                        if (dayFoodOfDay.GetCategories(dbContext).Any(x => x.Name == "Vegetariskt"))
                        {
                            hasVego = true;
                            break;
                        }
                    }

                    if (!hasVego)
                    {
                        break;
                    }
                    else
                    {
                        totalVego++;
                    }
                }
                if (totalVego >= 10)
                {
                    achievementRepository.EarnAchievement(AchievementRepository.AnimalFriend, Person.Jens.UserName);
                    achievementRepository.EarnAchievement(AchievementRepository.AnimalFriend, Person.Anna.UserName);
                }
            }

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
                    return Goal < Completed ? "color: #dc5a67;" : "color: #3ca878;";
                }

                return Goal <= Completed ? "color: #3ca878;" : "color: #dc5a67;";
            }
        }
    }
}
