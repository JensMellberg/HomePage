using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class FoodStorageModel : PageModel
    {
        public List<IngredientInstance> CurrentIngredients { get; set; }

        public ActionResult OnGet()
        {
            this.TryLogIn();
            this.CurrentIngredients = new FoodStorageRepository().GetIngredients(new IngredientRepository()).OrderBy(x => x.Ingredient.Name).ToList();

            return Page();
        }

        public ActionResult OnPost(string deleteId)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return BadRequest();
            }

            new FoodStorageRepository().Delete(deleteId);
            return new JsonResult(new { success = true });
        }
    }
}
