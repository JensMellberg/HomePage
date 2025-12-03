using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class IngredientsModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<Ingredient> Ingredients { get; set; }
        public void OnGet()
        {
            Ingredients = dbContext.Ingredient.OrderBy(x => x.CategoryId).ThenBy(x => x.Name).ToList();
        }
    }
}
