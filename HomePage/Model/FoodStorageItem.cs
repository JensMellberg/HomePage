using System.ComponentModel.DataAnnotations;
using System.Globalization;
using HomePage.Data;

namespace HomePage.Model
{
    public class FoodStorageItem
    {
        public static FoodStorageItem CreateFromIngredient(Ingredient ingredient, string amount, string unit)
        {
            var doubleAmount = double.Parse(amount.Replace(',', '.'), CultureInfo.InvariantCulture);
            var unitInstance = UnitTypes.CreateInstance(ingredient.UnitType, doubleAmount, unit);
            var foodStorageItem = new FoodStorageItem
            {
                IngredientId = ingredient.Id,
                Amount = unitInstance.Amount
            };

            return foodStorageItem;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        [SaveProperty]
        public Guid IngredientId { get; set; }

        public Ingredient Ingredient { get; set; }

        [SaveProperty]
        public double Amount { get; set; }

        public IngredientInstance ToIngredientInstance() => IngredientInstance.Create(Ingredient, Amount, null);
    }
}
