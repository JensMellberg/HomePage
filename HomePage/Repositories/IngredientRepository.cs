using HomePage.Data;
using HomePage.Model;

namespace HomePage.Repositories
{
    public class IngredientRepository(AppDbContext dbContext)
    {
        public string ClientEncodedList(HashSet<Guid> excluded = null)
        {
            excluded ??= [];
            var items = dbContext.Ingredient.Where(x => !excluded.Contains(x.Id)).OrderBy(x => x.Name);
            return string.Join("¤", items.Select(x => x.Id.ToString().ToLower() + "|" + x.Name + "|" + x.UnitType + "|" + x.CategoryId + "|" + x.StandardAmount + "|" + x.StandardUnit));
        }

        public Dictionary<Guid, Ingredient> IngredientsById = dbContext.Ingredient.ToDictionary(x => x.Id, x => x);
    }
}
