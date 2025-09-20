using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CategoryModel : PageModel
    {
        public Category Category { get; set; }
        public IActionResult OnGet(string categoryId)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            if (string.IsNullOrEmpty(categoryId))
            {
                Category = new Category();
            }
            else
            {
                Category = new CategoryRepository().TryGetValue(categoryId) ?? new Category();
            }

            return Page();
        }

        public IActionResult OnPost(string foodId, string categoryId, string name, string weekgoal, string weekgoalmax, string needsOnAllSides)
        {
            if (foodId != null)
            {
                var food = new FoodRepository().TryGetValue(foodId);
                var allCategories = new CategoryRepository().GetValues().Values.ToList();
                var foodCategories = allCategories.Select(x => new FoodCategory
                {
                    id = x.Key,
                    name = x.Name,
                    isSelected = food?.CategoriyIds.Contains(x.Key) == true
                });

                return new JsonResult(foodCategories);
            }
            else
            {
                var category = new Category { 
                    Key = categoryId, Name = name,
                    GoalPerWeek = string.IsNullOrEmpty(weekgoal) ? (string.IsNullOrEmpty(weekgoalmax) ? 0 : int.Parse(weekgoalmax)) : int.Parse(weekgoal),
                    IsBad = !string.IsNullOrEmpty(weekgoalmax),
                    NeedsOnAllSides = needsOnAllSides == "on"
                };
                new CategoryRepository().SaveValue(category);
            }

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
