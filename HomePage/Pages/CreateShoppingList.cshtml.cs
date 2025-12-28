using System.Text.Json;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateShoppingListModel(AppDbContext dbContext, 
        FoodStorageRepository foodStorageRepository,
        IngredientRepository ingredientRepository,
        DayFoodRepository dayFoodRepository, 
        SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public string IngredientsJson { get; set; }

        public string SkippedIngredientsJson { get; set; }


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

            var ingredientsList = new List<(bool isSkipped, IngredientInstance ingredient)>();

            foreach (var ingredient in ingredients.Values.OrderBy(x => x.SortOrder))
            {
                var original = ingredient.Copy();
                if (foodStorageItems.TryGetValue(ingredient.IngredientId, out var foodStorageIngredient))
                {
                    ingredient.Subtract(foodStorageIngredient);
                }

                if (ingredient.IsNonZero)
                {
                    ingredientsList.Add((false, ingredient));
                }

                if (original.Amount.Amount != ingredient.Amount.Amount)
                {
                    ingredientsList.Add((true, original));
                }
            }

            IngredientsJson = JsonSerializer.Serialize(ingredientsList.OrderBy(x => x.isSkipped).Select(x =>
            {
                var displayValues = x.ingredient.GetDisplayValues();
                return new
                {
                    id = x.ingredient.IngredientId,
                    amount = displayValues.amount,
                    unit = displayValues.unit,
                    category = x.ingredient.Ingredient.CategoryId,
                    isSkipped = x.isSkipped
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
