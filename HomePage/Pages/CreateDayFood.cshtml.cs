using System.Text.RegularExpressions;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class CreateDayFoodModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger, SettingsRepository settingsRepository) : BasePage(signInRepository)
    {
        public required string DateKey { get; set; }

        public required string DateQs { get; set; }

        public required string DateQsFirstOfWeek { get; set; }

        public required DayFood DayFood { get; set; }

        public required List<Food> ExistingFood { get; set; }

        public required List<Food> SideDishes { get; set; }

        public Guid FoodIdFromQueryString { get; set; }

        public bool CanDelete { get; set; }

        public IActionResult OnGet(int year, int month, int day, Guid foodId)
        {
            var date = new DateTime(year, month, day);
            DateKey = DateHelper.ToKey(date);

            var existingDayFood = dbContext.DayFood.Include(x => x.FoodConnections).FirstOrDefault(x => x.Date == date);
            CanDelete = existingDayFood != null && CanBeDeleted(DateKey);
            DayFood = existingDayFood ?? new EmptyDayFood { Date = date };
            var allFoods = dbContext.Food.ToList();
            ExistingFood = allFoods
                .Where(x => !x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();
            SideDishes = allFoods
                .Where(x => x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();
            FoodIdFromQueryString = foodId;
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

            if (date.Date == DateHelper.DateNow)
            {
                settingsRepository.UpdateHomePageChange();
            }

            dbContext.SaveChanges();
            return Redirect($"/WeekFood?{DateHelper.FormatDateForQueryString(DateHelper.GetFirstOfWeek(date))}");
        }

        private bool CanBeDeleted(string day) => !dbContext.FoodRanking.Where(x => x.Date == DateHelper.FromKey(day)).Any();
    }
}
