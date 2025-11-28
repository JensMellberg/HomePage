using System.Globalization;
using System.Text;

namespace HomePage
{
    public class Food : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public bool IsSideDish { get; set; }

        [SaveProperty]
        public string RecipeUrl { get; set; }

        [SaveProperty]
        public string Notes { get; set; }

        [SaveProperty]
        public bool InFolder { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> CategoriyIds { get; set; } = new List<string>();

        public List<Category> Categories { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> Ingredients { get; set; } = new List<string>();

        public bool HasHistory { get; set; }

        public void UpdateHistory(IEnumerable<DayFood> dayFoods)
        {
            HasHistory = dayFoods.Any(x => x.FoodId == Key || x.SideDishIds.Contains(Key));
        }

        public string ClientEncodedIngredients => string.Join('¤', Ingredients);

        public List<IngredientInstance> GetParsedIngredients(Dictionary<string, Ingredient> allIngredients)
        {
            var result = new List<IngredientInstance>();
            foreach (var line in Ingredients)
            {
                var tokens = line.Split('|');
                var instance = IngredientInstance.Create(allIngredients, tokens[0], tokens[1].ToDouble(), tokens[2]);
                result.Add(instance);
            }

            return result;
        }

        public IEnumerable<string> GetParsedIngredientIds() => Ingredients.Select(x => x.Split('|')[0]);

        public static void LoadCategories(IEnumerable<Food> food)
        {
            var categories = new CategoryRepository().GetValues();
            foreach (var foodItem in food)
            {
                foodItem.Categories = (foodItem.CategoriyIds ?? []).Select(x => categories[x]).ToList();
            }
        }
    }
}
