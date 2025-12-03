using HomePage.Data;
using HomePage.Model;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Repositories
{
    public class FoodRepository(AppDbContext dbContext)
    {
       public List<Food> GetPopulatedFood => dbContext.Food
                .Include(x => x.FoodConnections)
                .ThenInclude(x => x.DayFood)
                .Include(x => x.Categories)
                .Include(x => x.FoodIngredients)
                .ThenInclude(x => x.Ingredient)
                .AsSplitQuery()
                .ToList();
    }
}
