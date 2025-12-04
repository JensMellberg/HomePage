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

        public bool IsStandard { get; set; }

        public double StandardAmount { get; set; } = 1;

        public string? StandardUnit { get; set; }

        public IngredientCategory Category => IngredientCategory.Create(CategoryId);
    }
}
