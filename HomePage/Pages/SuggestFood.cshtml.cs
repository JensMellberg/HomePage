using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    public class SuggestFoodModel(AppDbContext dbContext,
        SignInRepository signInRepository,
        FoodRepository foodRepository,
        FoodStorageRepository foodStorageRepository,
        IngredientRepository ingredientRepository) : BasePage(signInRepository)
    {
        public List<FoodSuggestion> Suggestions { get; set; } = [];

        public required string CurrentCategories { get; set; }

        public required string CurrentIngredients { get; set; }

        public required string PossibleIngredients { get; set; }

        public IActionResult OnGet(string categoryFilter, string ingredientFilter)
        {
            var categoriesList = categoryFilter?.Split(',').ToHashSet();
            var ingredientsList = ingredientFilter?.Split(',').Select(Guid.Parse).ToHashSet();
            CurrentCategories = categoryFilter ?? "";
            CurrentIngredients = ingredientFilter ?? "";
            PossibleIngredients = ingredientRepository.ClientEncodedList();

            var allFood = foodRepository.GetPopulatedFood
                .Where(x => categoriesList == null || categoriesList.All(c => x.Categories.Select(x => x.Key).Contains(c)))
                .Where(x => ingredientsList == null || x.ContainsAllIngredients(ingredientsList));

            var storageIngredients = foodStorageRepository.GetIngredients().ToDictionary(x => x.IngredientId!.Value, x => x);
            foreach (var food in allFood.Where(x => !x.IsSideDish && x.FoodIngredients.Count > 0))
            {
                var missingIngredients = new List<IngredientInstance>();
                var totalIngredients = food.FoodIngredients.Where(x => !x.Ingredient.IsStandard).Count();
                foreach (var ingredient in food.FoodIngredients.Select(x => x.ToIngredientInstance()))
                {
                    if (storageIngredients.TryGetValue(ingredient.IngredientId!.Value, out var storageIngredient))
                    {
                        ingredient.Subtract(storageIngredient);
                    }

                    if (ingredient.IsNonZero)
                    {
                        missingIngredients.Add(ingredient);
                    }
                }

                Suggestions.Add(new() { Food = food, MissingIngredients = missingIngredients, RankingScore = (double)(missingIngredients.Count - totalIngredients) / totalIngredients });
            }

            return Page();
        }
    }

    public class FoodSuggestion
    {
        public Food Food { get; set; }

        public List<IngredientInstance> MissingIngredients { get; set; }

        public double RankingScore { get; set; }
    }
}
