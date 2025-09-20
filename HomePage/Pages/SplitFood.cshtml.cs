using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HomePage.Pages
{
    public class SplitFoodModel : PageModel
    {
        public List<Food> ExistingFood { get; set; }

        public List<Food> SideDishes { get; set; }

        public string FoodId { get; set; }
        public IActionResult OnGet(string foodId)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            FoodId = foodId;
            var allFoods = new FoodRepository().GetValues().Values;
            ExistingFood = allFoods
                .Where(x => !x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();
            SideDishes = allFoods
                .Where(x => x.IsSideDish)
                .OrderBy(x => x.Name)
                .ToList();

            return Page();
        }

        public IActionResult OnPost(string foodId, string newFood, string sideDish)
        {
            var dayFoodRepo = new DayFoodRepository();
            var rankingRepo = new FoodRankingRepository();
            var allDayFoods = dayFoodRepo.GetValues().Values;
            var allRankings = rankingRepo.GetValues().Values;
            foreach (var dayFood in allDayFoods.Where(x => x.FoodId == foodId))
            {
                dayFood.FoodId = newFood;
                dayFood.SideDishIds = [sideDish];
            }

            foreach (var ranking in allRankings.Where(x => x.FoodId == foodId))
            {
                ranking.FoodId = newFood;
            }

            rankingRepo.SaveValues(allRankings.ToDictionary(x => x.Key, x => x));
            dayFoodRepo.SaveValues(allDayFoods.ToDictionary(x => x.Key, x => x));
            new FoodRepository().Delete(foodId);

            return Redirect($"/AllFoods");
        }
    }
}
