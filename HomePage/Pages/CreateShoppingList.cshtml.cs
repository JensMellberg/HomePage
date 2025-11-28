using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateShoppingListModel : PageModel
    {
        public string ShoppingList { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public string IngredientsJson { get; set; }


        public IActionResult OnGet(string from, string to)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("Login");
            }

            PossibleIngredients = new IngredientRepository().ClientEncodedList();

            var fromDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(from));
            var toDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(to));
            var includedDayFoods = new DayFoodRepository().GetValues().Values.Where(x => x.Day >= fromDate && x.Day <= toDate);
            var allFoods = new FoodRepository().GetValues();
            var allIngredients = new IngredientRepository().GetValues();
            var ingredients = new Dictionary<string, IngredientInstance>();
            foreach (var dayFood in includedDayFoods)
            {
                dayFood.LoadSideDishes(allFoods);
                dayFood.Food = allFoods[dayFood.FoodId];
                foreach (var sideDish in dayFood.SideDishes)
                {
                    AddIngredientsFromFood(sideDish, dayFood.PortionMultiplier);
                }

                AddIngredientsFromFood(dayFood.Food, dayFood.PortionMultiplier);
            }

            // Remove existing items from food storage.
            var foodStorageItems = new FoodStorageRepository().GetIngredients(allIngredients)
                .ToDictionary(x => x.IngredientId, x => x);
            foreach (var standardItem in allIngredients.Values.Where(x => x.IsStandard && !ingredients.ContainsKey(x.Key) && !foodStorageItems.ContainsKey(x.Key)))
            {
                var instance = IngredientInstance.Create(allIngredients, standardItem.Key, standardItem.StandardAmount, standardItem.StandardUnit);
                ingredients.Add(standardItem.Key, instance);
            }

            var sb = new StringBuilder();
            var ingredientsList = new List<IngredientInstance>();

            foreach (var ingredient in ingredients.Values.OrderBy(x => x.SortOrder))
            {
                if (foodStorageItems.TryGetValue(ingredient.IngredientId, out var foodStorageIngredient))
                {
                    ingredient.Subtract(foodStorageIngredient);
                }

                if (ingredient.IsNonZero)
                {
                    sb.Append(ingredient.Ingredient.Name);
                    sb.Append(' ');
                    sb.Append(ingredient.GetStaticDisplayString());
                    sb.Append("\n");
                    ingredientsList.Add(ingredient);
                }
            }

            ShoppingList = sb.ToString();
            IngredientsJson = JsonSerializer.Serialize(ingredientsList.Select(x =>
            {
                var displayValues = x.GetDisplayValues();
                return new
                {
                    id = x.IngredientId,
                    amount = displayValues.amount,
                    unit = displayValues.unit,
                    category = x.Ingredient.CategoryId
                };
            }));

            return Page();

            void AddIngredientsFromFood(Food food, double multipler)
            {
                foreach (var ingredient in food.GetParsedIngredients(allIngredients))
                {
                    ingredient.MultiplyAmount(multipler);
                    AddIngredient(ingredient);
                }
            }

            void AddIngredient(IngredientInstance instance)
            {
                if (!ingredients.TryAdd(instance.IngredientId, instance))
                {
                    ingredients[instance.IngredientId].Combine(instance);
                }
            }
        }

        public IActionResult OnPost(List<IngredientModel> ingredients, string action)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return BadRequest();
            }

            var repo = new FoodStorageRepository();
            var allIngredients = new IngredientRepository().GetValues();
            var storageItems = repo.GetValues().Values.Select(x => x.ToIngredientInstance(allIngredients)).ToDictionary(x => x.IngredientId, x => x);
            foreach (var ingr in ingredients)
            {
                var doubleAmount = ingr.Amount.ToDouble();
                var instance = IngredientInstance.Create(allIngredients, ingr.Id, doubleAmount, ingr.Unit);
                if (storageItems.TryGetValue(instance.IngredientId, out var storageItem))
                {
                    storageItem.Combine(instance);
                }
                else
                {
                    storageItems.Add(instance.IngredientId, instance);
                }
            }

            var itemsToStore = storageItems.Values.Select(x => new FoodStorageItem
            {
                IngredientId = x.IngredientId,
                Amount = x.Amount.Amount
            });

            repo.SaveValues(itemsToStore.ToDictionary(x => x.IngredientId, x => x));
            return new JsonResult(new { success = true });
        }

    }

    public class IngredientModel
    {
        public string Id { get; set; }

        public string Amount { get; set; }

        public string Unit { get; set; }

        public string Category { get; set; }
    }
}
