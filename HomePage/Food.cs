using System.Text;

namespace HomePage
{
    public class Food : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

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

        public string ParsedIngredients
        {
            get
            {
                if (Ingredients?.Any() == true)
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < Ingredients.Count; i+=2)
                    {
                        if (i > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(Ingredients[i]);
                        sb.Append(":");
                        sb.Append(Ingredients[i + 1]);
                    }

                    return sb.ToString();
                }

                return string.Empty;
            }
        }

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
