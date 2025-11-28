using System.Linq;

namespace HomePage
{
    public class DayFood : SaveableItem
    {
        public string Key
        {
            get
            {
                return DateHelper.ToKey(Day);
            }
            set
            {
                Day = DateHelper.FromKey(value);
            }
        }

        public DateTime Day { get; set; }

        [SaveProperty]
        public string FoodId { get; set; }

        [SaveProperty]
        public bool IsVego { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> SideDishIds { get; set; } = new List<string>();

        public Food Food { get; set; }

        public List<Food> SideDishes { get; set; }

        [SaveProperty]
        public double Portions { get; set; }

        public double PortionMultiplier => Portions == 0 ? 1 : Portions;

        public void LoadSideDishes(Dictionary<string, Food> allFoods)
        {
            SideDishes = SideDishIds.Select(x => allFoods[x]).ToList();
        }

        public List<Category> GetCategories(Dictionary<string, Category> allCategories)
        {
            var result = Food.CategoriyIds.ToHashSet();
            foreach (var cats in SideDishes.Select(x => x.CategoriyIds.ToHashSet()))
            {
                foreach (var category in cats)
                {
                    var categoryItem = allCategories[category];
                    if (!result.Contains(category) && !categoryItem.NeedsOnAllSides)
                    {
                        result.Add(category);
                    }
                }

                var catsToRemove = new HashSet<string>();
                foreach (var category in result)
                {
                    var categoryItem = allCategories[category];
                    if (categoryItem.NeedsOnAllSides && !cats.Contains(category))
                    {
                        catsToRemove.Add(category);
                    }
                }

                result = result.Where(x => !catsToRemove.Contains(x)).ToHashSet();
            }

            if (IsVego)
            {
                var vegoCategory = allCategories.Values.FirstOrDefault(x => x.Name == "Vegetariskt");
                if (vegoCategory != null && !result.Contains(vegoCategory.Key))
                {
                    result.Add(vegoCategory.Key);
                }

                RemoveIfExists("Rött kött");
                RemoveIfExists("Chark");
                RemoveIfExists("Fläsk");
            }
            return result.Select(x => allCategories[x]).ToList();

            void RemoveIfExists(string name)
            {
                var category = allCategories.Values.FirstOrDefault(x => x.Name == name);
                if (category != null && result.Contains(category.Key))
                {
                    result.Remove(category.Key);
                }
            }
        }

        public string CombinedName => string.Join(" + ", Enumerable.Empty<Food>().Concat([Food]).Concat(SideDishes).Select(x => x.Name)) + 
            (IsVego ? " (vego)" : "");
    }
}
