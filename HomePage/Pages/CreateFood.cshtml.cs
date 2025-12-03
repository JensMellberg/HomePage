using System.Text.Json;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class CreateFoodModel(IngredientRepository ingredientRepository, AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Food Food { get; set; }

        public string DateKey { get; set; }

        public bool IsNew { get; set; }

        public bool CanSplit { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());


        public IActionResult OnGet(Guid foodId, string date)
        {
            DateKey = date;
            if (foodId == Guid.Empty)
            {
                Food = new Food();
                IsNew = true;
            }
            else
            {
                Food = dbContext.Food.Include(x => x.FoodIngredients).ThenInclude(x => x.Ingredient).Include(x => x.Categories).Include(x => x.FoodConnections).FirstOrDefault(x => x.Id == foodId) ?? new Food();
                IsNew = !CanDelete(Food);
                CanSplit = IsNew;
            }

            PossibleIngredients = ingredientRepository.ClientEncodedList();
            return Page();
        }

        public IActionResult OnPost(Guid foodId, string foodName, string date, string recipeUrl, string categories, string delete, string notes, string ingredients, string inFolder, string isSideDish)
        {
            var existing = dbContext.Food
                .Include(x => x.FoodConnections)
                .Include(x => x.Categories)
                .Include(x => x.FoodIngredients)
                .FirstOrDefault(x => x.Id == foodId);
            if (!string.IsNullOrEmpty(delete) && existing != null && CanDelete(existing))
            {
                dbContext.Food.Remove(existing);
            }
            else if (!string.IsNullOrEmpty(foodName))
            {
                var categoriesList = categories?.Split(',') ?? [];
                var categoryModels = dbContext.Category.Where(x => categoriesList.Contains(x.Key)).ToList();
                var parsedIngredients = ingredients?.Split('¤')?.Select(FoodIngredient.Parse)?.ToList() ?? [];

                if (existing != null)
                {
                    existing.Name = foodName;
                    existing.RecipeUrl = recipeUrl;
                    existing.Categories = categoryModels;
                    existing.Notes = notes;
                    existing.FoodIngredients = parsedIngredients;
                    existing.InFolder = inFolder == "on";
                    existing.IsSideDish = isSideDish == "on";
                } else
                {
                    var food = new Food
                    {
                        Id = foodId,
                        Name = foodName,
                        RecipeUrl = recipeUrl,
                        Categories = categoryModels,
                        Notes = notes,
                        FoodIngredients = parsedIngredients,
                        InFolder = inFolder == "on",
                        IsSideDish = isSideDish == "on"
                    };

                    dbContext.Food.Add(food);
                }
            }

            dbContext.SaveChanges();
            
            if (!string.IsNullOrEmpty(date))
            {
                return Redirect($"/CreateDayFood?{DateHelper.FormatDateForQueryString(DateHelper.FromKey(date))}&foodId={foodId}");
            }

            return Redirect($"/AllFoods");
        }

        private static bool CanDelete(Food food)
        {
            return !food.HasHistory;
        }
    }
}
