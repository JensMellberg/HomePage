using HomePage.Data;
using HomePage.Model;

namespace HomePage.Pages
{
    public class AllCategoriesModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<Category> AllCategories { get; set; }

        public void OnGet()
        {
            AllCategories = dbContext.Category
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
