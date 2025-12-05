using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateShoppingListModel(AppDbContext dbContext, 
        FoodStorageRepository foodStorageRepository,
        IngredientRepository ingredientRepository,
        DayFoodRepository dayFoodRepository, 
        SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public string ShoppingList { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public string IngredientsJson { get; set; }


        public IActionResult OnGet(string from, string to)
        {
            PossibleIngredients = ingredientRepository.ClientEncodedList();

            var fromDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(from));
            var toDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(to));
            var includedDayFoods = dayFoodRepository.GetPopulatedDayFood().Where(x => x.Date >= fromDate && x.Date <= toDate);
            var allIngredients = ingredientRepository.IngredientsById;
            var ingredients = new Dictionary<Guid, IngredientInstance>();
            foreach (var dayFood in includedDayFoods)
            {
                foreach (var sideDish in dayFood.SideDishes)
                {
                    AddIngredientsFromFood(sideDish, dayFood.Portions);
                }

                AddIngredientsFromFood(dayFood.MainFood, dayFood.Portions);
            }

            // Remove existing items from food storage.
            var foodStorageItems = foodStorageRepository.GetIngredients()
                .ToDictionary(x => x.IngredientId, x => x);
            foreach (var standardItem in allIngredients.Values.Where(x => x.IsStandard && !ingredients.ContainsKey(x.Id) && !foodStorageItems.ContainsKey(x.Id)))
            {
                var instance = IngredientInstance.Create(allIngredients[standardItem.Id], standardItem.StandardAmount, standardItem.StandardUnit);
                ingredients.Add(standardItem.Id, instance);
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
                var parsed = food.FoodIngredients.Select(x => x.ToIngredientInstance());
                foreach (var ingredient in parsed)
                {
                    ingredient.MultiplyAmount(multipler);
                    AddIngredient(ingredient);
                }
            }

            void AddIngredient(IngredientInstance instance)
            {
                if (!ingredients.TryAdd(instance.IngredientId ?? Guid.Empty, instance))
                {
                    ingredients[instance.IngredientId ?? Guid.Empty].Combine(instance);
                }
            }
        }

        public IActionResult OnPost(List<IngredientModel> ingredients, string action)
        {
            var redirectResult = GetPotentialClientRedirectResult(true, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var storageItems = foodStorageRepository.GetIngredients().ToDictionary(x => x.IngredientId, x => x);
            var allIngredients = ingredientRepository.IngredientsById;
            foreach (var ingr in ingredients)
            {
                var doubleAmount = ingr.Amount.ToDouble();
                var instance = IngredientInstance.Create(allIngredients[ingr.Id], doubleAmount, ingr.Unit);
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
                IngredientId = x.IngredientId ?? Guid.Empty,
                Amount = x.Amount.Amount
            });

            dbContext.FoodStorage.RemoveRange(dbContext.FoodStorage);
            dbContext.FoodStorage.AddRange(itemsToStore);
            dbContext.SaveChanges();
            return Utils.CreateClientResult(new { success = true });
        }

    }

    public class IngredientModel
    {
        public Guid Id { get; set; }

        public string Amount { get; set; }

        public string Unit { get; set; }

        public string Category { get; set; }
    }
}
