using System.Text.Json;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CreateFoodStorageModel(AppDbContext dbContext, IngredientRepository ingredientRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public IngredientInstance Ingredient { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public (string amount, string unit) DisplayValues;

        public IActionResult OnGet(Guid id)
        {
            var usedIngredients = dbContext.FoodStorage.Select(x => x.IngredientId).ToHashSet();
            if (id == Guid.Empty)
            {
                Ingredient = new IngredientInstance();
            }
            else
            {
                var existing = dbContext.FoodStorage.Include(x => x.Ingredient).Single(x => x.IngredientId == id);
                Ingredient = existing.ToIngredientInstance();
                usedIngredients.Remove(existing.IngredientId);
            }
            
            PossibleIngredients = ingredientRepository.ClientEncodedList(usedIngredients);
            DisplayValues = Ingredient.GetDisplayValues();

            return Page();
        }

        public IActionResult OnPost(Guid ingredientId, string unit, string amount)
        {
            var ingredient = dbContext.Ingredient.FirstOrDefault(x => x.Id == ingredientId);
            if (ingredient == null)
            {
                return NotFound();
            }

            var foodStorageItem = FoodStorageItem.CreateFromIngredient(ingredient, amount, unit);
            var existing = dbContext.FoodStorage.FirstOrDefault(x => x.IngredientId == ingredientId);
            if (existing != null)
            {
                dbContext.FoodStorage.Remove(existing);
            }

            dbContext.FoodStorage.Add(foodStorageItem);
            dbContext.SaveChanges();
            return Redirect($"/FoodStorage");
        }
    }
}
