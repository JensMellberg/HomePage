using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ExcludedGoodie
    {
        [Key]
        public Guid FoodId { get; set; }
    }
}
