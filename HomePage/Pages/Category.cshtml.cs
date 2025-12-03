using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CategoryModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Category Category { get; set; }
        public IActionResult OnGet(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                Category = new Category();
            }
            else
            {
                Category = dbContext.Category.Find(categoryId) ?? new Category();
            }

            return Page();
        }

        public IActionResult OnPost(Guid? foodId, string categoryId, string name, string weekgoal, string weekgoalmax, string needsOnAllSides)
        {
            if (foodId != null)
            {
                var food = dbContext.Food.Include(x => x.Categories).FirstOrDefault(x => x.Id == foodId);
                var allCategories = dbContext.Category.ToList();
                var foodCategories = allCategories.Select(x => new FoodCategory
                {
                    id = x.Key,
                    name = x.Name,
                    isSelected = food?.Categories.Any(c => x.Key == c.Key) == true
                });

                return new JsonResult(foodCategories);
            }

            var existing = dbContext.Category.Find(categoryId);
            var goalPerWeek = string.IsNullOrEmpty(weekgoal) ? (string.IsNullOrEmpty(weekgoalmax) ? 0 : int.Parse(weekgoalmax)) : int.Parse(weekgoal);
            var IsBad = !string.IsNullOrEmpty(weekgoalmax);
            if (existing != null)
            {
                existing.Name = name;
                existing.GoalPerWeek = goalPerWeek;
                existing.IsBad = IsBad;
                existing.NeedsOnAllSides = needsOnAllSides == "on";
            }
            else
            {
                var category = new Category
                {
                    Key = categoryId,
                    Name = name,
                    GoalPerWeek = goalPerWeek,
                    IsBad = IsBad,
                    NeedsOnAllSides = needsOnAllSides == "on"
                };
                dbContext.Category.Add(category);
            }

            dbContext.SaveChanges();

            return Redirect($"/AllCategories");
        }

        private class FoodCategory
        {
            public string id { get; set; }

            public string name { get; set; }

            public bool isSelected { get; set; }
        }
    }
}
