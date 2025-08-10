using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CreateFoodModel : PageModel
    {
        public Food Food { get; set; }

        public string DateKey { get; set; }

        public bool IsNew { get; set; }

        public IActionResult OnGet(string foodId, string date)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            DateKey = date;
            if (string.IsNullOrEmpty(foodId))
            {
                Food = new Food();
                IsNew = true;
            }
            else
            {
                Food = new FoodRepository().TryGetValue(foodId) ?? new Food();
                IsNew = !this.CanDelete(foodId);
            }

            Food.LoadCategories([Food]);
            return Page();
        }

        public IActionResult OnPost(string foodId, string foodName, string date, string recipeUrl, string categories, string delete, string notes, string ingredients, string inFolder)
        {
            if (!string.IsNullOrEmpty(delete) && this.CanDelete(foodId))
            {
                new FoodRepository().Delete(foodId);
            }
            else if (!string.IsNullOrEmpty(foodName))
            {
                var categoriesList = categories?.Split(',').ToList();
                /*var ingredientsList = ingredients?.Split(',').Select(x => x.Split(':')) ?? [];
                var actualIngredientsList = new List<string>();
                foreach (var pair in ingredientsList)
                {
                    var ingredient = pair[0].Trim();
                    var amount = pair[1].Trim();
                    actualIngredientsList.Add(ingredient);
                    actualIngredientsList.Add(amount);
                }*/
                var food = new Food 
                { 
                    Key = foodId, 
                    Name = foodName, 
                    RecipeUrl = recipeUrl, 
                    CategoriyIds = categoriesList, 
                    Notes = notes,
                    /*Ingredients = actualIngredientsList*/
                    InFolder = inFolder == "on"
                };
                new FoodRepository().SaveValue(food);
            }
            
            if (!string.IsNullOrEmpty(date))
            {
                return Redirect($"/CreateDayFood?{DateHelper.FormatDateForQueryString(DateHelper.FromKey(date))}&foodId={foodId}");
            }

            return Redirect($"/AllFoods");
        }

        private bool CanDelete(string foodId)
        {
            return !new DayFoodRepository().GetValues().Values.Any(x => x.FoodId == foodId);
        }
    }
}
