namespace HomePage
{
    public class FoodStorageRepository : Repository<FoodStorageItem>
    {
        public override string FileName => "FoodStorage.txt";

        public List<IngredientInstance> GetIngredients(IngredientRepository ingredientRepository) => GetValues().Values
                .Select(x => x.ToIngredientInstance(ingredientRepository))
                .ToList();

        public List<IngredientInstance> GetIngredients(Dictionary<string, Ingredient> allIngredients) => GetValues().Values
                .Select(x => x.ToIngredientInstance(allIngredients))
                .ToList();
    }
}
