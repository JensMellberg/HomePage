using HomePage.Data;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class FoodStorageModel(FoodStorageRepository foodStorageRepository, AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<IngredientInstance> CurrentIngredients { get; set; }

        public ActionResult OnGet()
        {
            CurrentIngredients = foodStorageRepository.GetIngredients().OrderBy(x => x.Ingredient.Name).ToList();

            return Page();
        }

        public ActionResult OnPost(Guid deleteId)
        {
            if (!IsAdmin)
            {
                return BadRequest();
            }

            var toDelete = dbContext.FoodStorage.Single(x => x.IngredientId == deleteId);
            dbContext.FoodStorage.Remove(toDelete);
            dbContext.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}
