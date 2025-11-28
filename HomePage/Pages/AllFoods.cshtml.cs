using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public enum Sorting
    {
        Name,
        JensRating,
        AnnaRating,
        TotalRating
    }

    public class AllFoodsModel : PageModel
    {
        public List<Food> AllFoods { get; set; }

        public List<Food> SideDishes { get; set; }

        public Dictionary<string, string[]> FoodRankings { get; set; } = new();

        public (Sorting, string)[] SortingOptions = [
            (Sorting.Name, "Maträtt"),
            (Sorting.JensRating, "Jens betyg"),
            (Sorting.AnnaRating, "Annas betyg"),
            (Sorting.TotalRating, "Totalt betyg")
        ];

        public Sorting CurrentSorting { get; set; } = Sorting.Name;

        public string CurrentCategories { get; set; }

        public string CurrentFilter { get; set; }

        public string CurrentIngredients { get; set; }

        public string PossibleIngredients { get; set; }

        public void OnGet(string filter, string categoryFilter, string sorting, string ingredientFilter)
        {
            this.TryLogIn();
            CurrentFilter = filter;
            Enum.TryParse(typeof(Sorting), sorting, out var parsedSorting);
            if (parsedSorting is Sorting correctlyParsed) {
                CurrentSorting = correctlyParsed;
            }

            var Regex = string.IsNullOrEmpty(filter) ? null : new Regex("^.*" + filter + ".*$", RegexOptions.IgnoreCase);
            var categoriesList = categoryFilter?.Split(',').ToHashSet();
            var ingredientsList = ingredientFilter?.Split(',').ToHashSet();
            var foodValues = new FoodRepository().GetValues().Select(x => x.Value);
            var ingredientsRepository = new IngredientRepository();
            PossibleIngredients = ingredientsRepository.ClientEncodedList();
            var allIngredients = ingredientsRepository.GetValues();

            var allFoods = foodValues
                .Where(x => !x.IsSideDish)
                .Where(x => Regex?.IsMatch(x.Name) ?? true)
                .Where(x => categoriesList == null || categoriesList.All(c => x.CategoriyIds.Contains(c)))
                .Where(x => ingredientsList == null || FoodContainsAllIngredients(x, ingredientsList));

            SideDishes = foodValues
                .Where(x => x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();

            CurrentCategories = categoryFilter ?? "";
            CurrentIngredients = ingredientFilter ?? "";
            var allRankings = new FoodRankingRepository().GetValues().Values;
            var foodKeys = new Dictionary<Food, string>();
            var rankingsKeys = new Dictionary<Food, double>();
            foreach (var food in allFoods)
            {
                var relevantRankings = allRankings.Where(x => x.FoodId == food.Key);
                Utils.CalculateAverages(relevantRankings, out var jensAverage, out var annaAverage, out var totalAverage);

                string[] strings = [
                    GetRankingString(Person.Anna.Name, annaAverage),
                    GetRankingString(Person.Jens.Name, jensAverage),
                    GetRankingString("Totalt", totalAverage)
                ];

                FoodRankings.Add(food.Key, strings);
                switch (CurrentSorting)
                {
                    case Sorting.Name: foodKeys[food] = food.Name; break;
                    case Sorting.JensRating: rankingsKeys[food] = jensAverage; break;
                    case Sorting.AnnaRating: rankingsKeys[food] = annaAverage; break;
                    case Sorting.TotalRating: rankingsKeys[food] = totalAverage; break;
                }
            }

            if (CurrentSorting == Sorting.Name)
            {
                AllFoods = [.. allFoods.OrderBy(x => foodKeys[x])];
            }
            else
            {
                AllFoods = [.. allFoods.OrderByDescending(x => rankingsKeys[x])];
            }

            var dayFoods = new DayFoodRepository().GetValues().Values;
            foreach (var food in AllFoods.Concat(SideDishes))
            {
                food.UpdateHistory(dayFoods);
            }

            string GetRankingString(string person, double average) => person + ": " + (average == 0 ? "-" : average.ToString());

            bool FoodContainsAllIngredients(Food food, HashSet<string> ingredients)
            {
                var parsedIngredients = food.GetParsedIngredientIds().ToHashSet();
                return ingredients.All(parsedIngredients.Contains);
            }
        }
    }
}
