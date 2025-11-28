using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateIngredientModel : PageModel
    {
        public Ingredient Ingredient { get; set; }

        public string Categories { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public IActionResult OnGet(string id)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                Ingredient = new Ingredient();
            }
            else
            {
                Ingredient = new IngredientRepository().TryGetValue(id) ?? new Ingredient();
            }

            Categories = string.Join(',', IngredientCategory.Categories.OrderBy(x => x));
            return Page();
        }

        public IActionResult OnPost(string id, string name, string unitType, string category, string isstandard, string standardamount, string standardunit)
        {
            var ingredient = new Ingredient {
                Key = id,
                Name = name,
                UnitType = unitType,
                CategoryId = category,
                IsStandard = isstandard == "on",
                StandardAmount = standardamount.ToDouble(),
                StandardUnit = standardunit
            };

            new IngredientRepository().SaveValue(ingredient);

            return Redirect($"/Ingredients");
        }
    }
}
