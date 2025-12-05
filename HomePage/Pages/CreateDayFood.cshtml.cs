using System.Text.RegularExpressions;
using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class CreateDayFoodModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public string DateKey { get; set; }

        public string DateQs { get; set; }

        public string DateQsFirstOfWeek { get; set; }

        public string CurrentFilter { get; set; }

        public DayFood? DayFood { get; set; }

        public List<Food> ExistingFood { get; set; }

        public List<Food> SideDishes { get; set; }

        public Guid FoodIdFromQueryString { get; set; }

        public bool CanDelete { get; set; }

        public IActionResult OnGet(int year, int month, int day, Guid foodId, string filter)
        {
            var Regex = string.IsNullOrEmpty(filter) ? null : new Regex("^.*" + filter + ".*$", RegexOptions.IgnoreCase);
            var date = new DateTime(year, month, day);
            DateKey = DateHelper.ToKey(date);

            var existingDayFood = dbContext.DayFood.Include(x => x.FoodConnections).FirstOrDefault(x => x.Date == date);
            CanDelete = existingDayFood != null && CanBeDeleted(DateKey);
            DayFood = existingDayFood ?? new EmptyDayFood { Date = date };
            var allFoods = dbContext.Food.ToList();
            ExistingFood = allFoods
                .Where(x => !x.IsSideDish)
                .Where(x => Regex?.IsMatch(x.Name) ?? true)
                .OrderBy(x => x.Name)
                .ToList();
            SideDishes = allFoods
                .Where(x => x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();
            FoodIdFromQueryString = foodId;
            CurrentFilter = filter;
            DateQs = DateHelper.FormatDateForQueryString(date);
            DateQsFirstOfWeek = DateHelper.FormatDateForQueryString(DateHelper.GetFirstOfWeek(date));
            return Page();
        }

        public IActionResult OnPost(string day, Guid foodId, string delete, string sideDishIds, string isVego, string portions)
        {
            var date = DateHelper.FromKey(day);
            var existingDayFood = dbContext.DayFood.Include(x => x.FoodConnections).ThenInclude(x => x.Food).FirstOrDefault(x => x.Date == date);
            if (!string.IsNullOrEmpty(delete) && existingDayFood != null && this.CanBeDeleted(day))
            {
                dbContext.DayFood.Remove(existingDayFood);
                logger.Information($"Deleted food {existingDayFood.CombinedName} on day {date.ToReadable()}.", LoggedInPerson?.UserName);
            }
            else
            {
                var idToUse = existingDayFood?.Id ?? Guid.NewGuid();
                var foodIds = (sideDishIds?.Split(',').Select(Guid.Parse) ?? []).Concat([foodId]).ToList();
                var foods = dbContext.Food.Where(x => foodIds.Contains(x.Id)).ToList();
                var foodConnections = foods.Select(x => new DayFoodDishConnection
                {
                    DayFoodId = idToUse,
                    FoodId = x.Id,
                    Food = x,
                    IsMainDish = x.Id == foodId
                }).ToList();

                if (existingDayFood != null)
                {
                    existingDayFood.FoodConnections = foodConnections;
                    existingDayFood.IsVego = isVego == "on";
                    existingDayFood.Portions = portions.ToDouble();
                    logger.Information($"Updated food {existingDayFood.CombinedName} for day {date.ToReadable()}.", LoggedInPerson?.UserName);
                }
                else
                {
                    var dayFood = new DayFood { Id = idToUse, Date = date, IsVego = isVego == "on", Portions = portions.ToDouble(), FoodConnections = foodConnections };
                    dbContext.DayFood.Add(dayFood);
                    logger.Information($"Added food {dayFood.CombinedName} for day {date.ToReadable()}.", LoggedInPerson?.UserName);
                }
            }

            dbContext.SaveChanges();
            return Redirect($"/WeekFood?{DateHelper.FormatDateForQueryString(DateHelper.GetFirstOfWeek(date))}");
        }

        private bool CanBeDeleted(string day) => !dbContext.FoodRanking.Where(x => x.Date == DateHelper.FromKey(day)).Any();
    }
}
