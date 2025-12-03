using System.Text.Json;
using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CreateIngredientModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Ingredient Ingredient { get; set; }

        public string Categories { get; set; }

        public string AllUnitValues = JsonSerializer.Serialize(UnitTypes.GetAllUnitValues());

        public IActionResult OnGet(Guid id)
        {
            if (id == Guid.Empty)
            {
                Ingredient = new Ingredient();
            }
            else
            {
                Ingredient = dbContext.Ingredient.Single(x => x.Id == id);
            }

            Categories = string.Join(',', IngredientCategory.Categories.OrderBy(x => x));
            return Page();
        }

        public IActionResult OnPost(Guid id, string name, string unitType, string category, string isstandard, string standardamount, string standardunit)
        {
            var ingredient = new Ingredient
            {
                Id = id,
                Name = name,
                UnitType = unitType,
                CategoryId = category,
                IsStandard = isstandard == "on",
                StandardAmount = standardamount.ToDouble(),
                StandardUnit = standardunit
            };
            var existing = dbContext.Ingredient.Find(id);

            if (existing == null)
            {
                dbContext.Ingredient.Add(ingredient);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(ingredient);
            }

            dbContext.SaveChanges();

            return Redirect($"/Ingredients");
        }
    }
}
