namespace HomePage
{
    public class IngredientRepository : Repository<Ingredient>
    {
        public override string FileName => "Ingredients.txt";

        public string ClientEncodedList(HashSet<string> excluded = null)
        {
            excluded ??= [];
            var items = GetValues().Values.Where(x => !excluded.Contains(x.Key)).OrderBy(x => x.Name);
            return string.Join("¤", items.Select(x => x.Key + "|" + x.Name + "|" + x.UnitType + "|" + x.CategoryId + "|" + x.StandardAmount + "|" + x.StandardUnit));
        }
    }
}
