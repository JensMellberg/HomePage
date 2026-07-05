using HomePage.Data;
using HomePage.Model;

namespace HomePage.Pages
{
    public class IngredientsModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<Ingredient> Ingredients { get; set; }
        public string CurrentFilter { get; set; }
        public void OnGet(string filter)
        {
            CurrentFilter = filter;
            Ingredients = dbContext.Ingredient
                .Where(x => filter == null || x.Name.Contains(filter))
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Name)
                .ToList();
        }
    }
}
