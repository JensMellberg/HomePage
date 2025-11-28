namespace HomePage
{
    public class IngredientInstance
    {
        public Ingredient Ingredient { get; set; }

        public UnitInstance Amount { get; set; }

        public int SortOrder => Ingredient.Category.SortOrder;

        public void MultiplyAmount(double multipler) => Amount.Multiply(multipler);

        public static IngredientInstance Create(IngredientRepository repository, string ingredientId, double amount, string unit) =>
            Create(repository.GetValues(), ingredientId, amount, unit);

        public static IngredientInstance Create(Dictionary<string, Ingredient> allIngredients, string ingredientId, double amount, string unit)
        {
            if (!allIngredients.TryGetValue(ingredientId, out var ingredient))
            {
                throw new Exception();
            }

            var unitInstance = UnitTypes.CreateInstance(ingredient.UnitType, amount);
            if (unit != null)
            {
                unitInstance.UpdateAmount(amount, unit);
            }

            return new IngredientInstance
            {
                Ingredient = ingredient,
                Amount = unitInstance
            };
        }

        public void Combine(IngredientInstance other)
        {
            Amount = Amount.Combine(other.Amount);
        }

        public void Subtract(IngredientInstance other)
        {
            Amount = Amount.Subtract(other.Amount);
        }

        public bool IsNonZero => Amount.Amount > 0;

        public string? IngredientId => Ingredient?.Key;

        public (string amount, string unit) GetDisplayValues() => Amount?.GetDisplayValues() ?? ("1", "st");

        public string GetStaticDisplayString()
        {
            (var amount, var unit) = GetDisplayValues();
            return $"{amount} {unit}";
        }
    }
}
