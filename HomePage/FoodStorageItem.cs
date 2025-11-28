using System.Globalization;
using System.Text;

namespace HomePage
{
    public class FoodStorageItem : SaveableItem
    {
        public string Key
        {
            get
            {
                return IngredientId;
            }
            set
            {
                IngredientId = value;
            }
        }

        public static FoodStorageItem CreateFromIngredient(Ingredient ingredient, string amount, string unit)
        {
            var doubleAmount = double.Parse(amount.Replace(',', '.'), CultureInfo.InvariantCulture);
            var unitInstance = UnitTypes.CreateInstance(ingredient.UnitType, doubleAmount, unit);
            var foodStorageItem = new FoodStorageItem
            {
                IngredientId = ingredient.Key,
                Amount = unitInstance.Amount
            };

            return foodStorageItem;
        }

        [SaveProperty]
        public string IngredientId { get; set; }

        [SaveProperty]
        public double Amount { get; set; }

        public IngredientInstance ToIngredientInstance(IngredientRepository repository) => IngredientInstance.Create(repository, IngredientId, Amount, null);

        public IngredientInstance ToIngredientInstance(Dictionary<string, Ingredient> allIngredients) => IngredientInstance.Create(allIngredients, IngredientId, Amount, null);
    }
}
