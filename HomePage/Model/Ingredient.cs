using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class Ingredient
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string UnitType { get; set; }

        [MaxLength(50)]
        public string CategoryId { get; set; }

        [SaveProperty]
        public bool IsStandard { get; set; }

        [SaveProperty]
        public double StandardAmount { get; set; } = 1;

        [SaveProperty]
        public string? StandardUnit { get; set; }

        public IngredientCategory Category => IngredientCategory.Create(CategoryId);
    }
}
