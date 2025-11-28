using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateFoodStorageModel : PageModel
    {
        public IngredientInstance Ingredient { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public (string amount, string unit) DisplayValues;

        public IActionResult OnGet(string id)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            var repo = new FoodStorageRepository();
            var usedIngredients = repo.GetValues().Values.Select(x => x.IngredientId).ToHashSet();
            if (string.IsNullOrEmpty(id))
            {
                Ingredient = new IngredientInstance();
            }
            else
            {
                var existing = repo.TryGetValue(id) ?? throw new Exception();
                Ingredient = existing.ToIngredientInstance(new IngredientRepository());
                usedIngredients.Remove(existing.IngredientId);
            }
            
            PossibleIngredients = new IngredientRepository().ClientEncodedList(usedIngredients);
            DisplayValues = Ingredient.GetDisplayValues();

            return Page();
        }

        public IActionResult OnPost(string ingredientId, string unit, string amount)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            var ingredient = new IngredientRepository().TryGetValue(ingredientId);
            if (ingredient == null)
            {
                return NotFound();
            }


            var foodStorageItem = FoodStorageItem.CreateFromIngredient(ingredient, amount, unit);
            new FoodStorageRepository().SaveValue(foodStorageItem);

            return Redirect($"/FoodStorage");
        }
    }
}
