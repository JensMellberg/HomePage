using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class Food
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string Name { get; set; }

        public bool IsSideDish { get; set; }

        public string? RecipeUrl { get; set; }

        public string? Notes { get; set; }

        public bool InFolder { get; set; }

        public List<Category> Categories { get; set; } = [];

        [NotMapped]
        public List<DayFood> MainFoodIn => FoodConnections.Where(x => x.IsMainDish).Select(x => x.DayFood).ToList();

        [NotMapped]
        public List<DayFood> SideDishIn => FoodConnections.Where(x => !x.IsMainDish).Select(x => x.DayFood).ToList();

        public List<DayFoodDishConnection> FoodConnections { get; set; } = [];

        public List<FoodIngredient> FoodIngredients { get; set; } = [];

        public string ClientEncodedIngredients => string.Join('¤', FoodIngredients.OrderBy(x => x.Ingredient.Category.SortOrder).Select(x => x.ClientEncode()));

        public bool HasHistory => MainFoodIn.Count != 0 || SideDishIn.Count != 0;

        public bool ContainsAllIngredients(HashSet<Guid> ingredients)
        {
            var foodIngredientIds = FoodIngredients.Select(x => x.IngredientId).ToHashSet();
            return ingredients.All(foodIngredientIds.Contains);
        }
    }

    public class FoodIngredient
    {
        public Guid FoodId { get; set; }

        public Food Food { get; set; }

        public Guid IngredientId { get; set; }

        public Ingredient Ingredient { get; set; }

        public double Amount { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; }

        public static FoodIngredient Parse(string text)
        {
            var tokens = text.Split('|');
            return new FoodIngredient
            {
                IngredientId = Guid.Parse(tokens[0]),
                Amount = tokens[1].ToDouble(),
                Unit = tokens[2],
            };
        }

        public string ClientEncode() => $"{IngredientId}|{Amount}|{Unit}|{Ingredient.Category.Name}";

        public IngredientInstance ToIngredientInstance() => IngredientInstance.Create(Ingredient, Amount, Unit);
    }
}
