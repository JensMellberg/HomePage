namespace HomePage
{
    public class Ingredient : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public string UnitType { get; set; }

        [SaveProperty]
        public string CategoryId { get; set; }

        [SaveProperty]
        public bool IsStandard { get; set; }

        [SaveProperty]
        public double StandardAmount { get; set; } = 1;

        [SaveProperty]
        public string StandardUnit { get; set; }

        public IngredientCategory Category => IngredientCategory.Create(CategoryId);
    }
}
