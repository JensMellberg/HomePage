using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class AllCategoriesModel : PageModel
    {
        public List<Category> AllCategories { get; set; }

        public void OnGet()
        {
            this.TryLogIn();
            AllCategories = new CategoryRepository().GetValues()
                .Select(x => x.Value)
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
