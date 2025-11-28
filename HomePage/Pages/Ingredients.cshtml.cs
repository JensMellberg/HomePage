using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class IngredientsModel : PageModel
    {
        public List<Ingredient> Ingredients { get; set; }
        public void OnGet()
        {
            this.TryLogIn();
            Ingredients = new IngredientRepository().GetValues().Values.OrderBy(x => x.CategoryId).ThenBy(x => x.Name).ToList();
        }
    }
}
