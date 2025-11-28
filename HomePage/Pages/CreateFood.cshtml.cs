using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CreateFoodModel : PageModel
    {
        public Food Food { get; set; }

        public string DateKey { get; set; }

        public bool IsNew { get; set; }

        public bool CanSplit { get; set; }

        public string PossibleIngredients { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());


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
                CanSplit = IsNew;
            }

            PossibleIngredients = new IngredientRepository().ClientEncodedList();

            Food.LoadCategories([Food]);
            return Page();
        }

        public IActionResult OnPost(string foodId, string foodName, string date, string recipeUrl, string categories, string delete, string notes, string ingredients, string inFolder, string isSideDish)
        {
            if (!string.IsNullOrEmpty(delete) && this.CanDelete(foodId))
            {
                new FoodRepository().Delete(foodId);
            }
            else if (!string.IsNullOrEmpty(foodName))
            {
                var categoriesList = categories?.Split(',').ToList();
                var parsedIngredients = ingredients?.Split('¤')?.ToList();
                var food = new Food
                { 
                    Key = foodId,
                    Name = foodName,
                    RecipeUrl = recipeUrl,
                    CategoriyIds = categoriesList,
                    Notes = notes,
                    Ingredients = parsedIngredients,
                    InFolder = inFolder == "on",
                    IsSideDish = isSideDish == "on"
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
            return !new DayFoodRepository().GetValues().Values.Any(x => x.FoodId == foodId || x.SideDishIds.Contains(foodId));
        }
    }
}
