using HomePage.Data;
using HomePage.Model;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Repositories
{
    public class DayFoodRepository(AppDbContext dbContext)
    {
        public List<DayFood> GetPopulatedDayFood()
        {
            var dayFoods = dbContext.DayFood
                 .Include(x => x.FoodConnections)
                 .ToList();

            var food = dbContext.Food
                .Include(x => x.Categories)
                .Include(x => x.FoodIngredients)
                .ThenInclude(x => x.Ingredient)
                .AsSplitQuery()
                .ToDictionary(x => x.Id, x => x);

            foreach (var dayFood in dayFoods)
            {
                foreach (var connection in dayFood.FoodConnections)
                {
                    connection.Food = food[connection.FoodId];
                }
            }

            return dayFoods;
        }


    }
}
