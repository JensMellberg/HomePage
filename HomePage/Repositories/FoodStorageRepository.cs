using HomePage.Data;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Repositories
{
    public class FoodStorageRepository(AppDbContext dbContext)
    {
        public List<IngredientInstance> GetIngredients() => dbContext.FoodStorage
                .Include(x => x.Ingredient)
                .Select(x => x.ToIngredientInstance())
                .ToList();
    }
}
