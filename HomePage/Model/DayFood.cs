using System.ComponentModel.DataAnnotations.Schema;
using HomePage.Data;

namespace HomePage.Model
{
    public class DayFoodDishConnection
    {
        public Guid DayFoodId { get; set; }

        public DayFood DayFood { get; set; }

        public Guid FoodId { get; set; }

        public Food Food { get; set; }

        public bool IsMainDish { get; set; }
    }

    public class EmptyDayFood : DayFood
    {
        public override Food MainFood
        {
            get { return new Food { Id = Guid.Empty }; }
            set { }
        }
    }

    public class DayFood
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Date { get; set; }

        [NotMapped]
        public Guid MainFoodId => MainFood.Id;

        [NotMapped]
        public virtual Food MainFood
        {
            get
            {
                return FoodConnections.FirstOrDefault(x => x.IsMainDish).Food;
            }
            set
            {
                FoodConnections.Add(new DayFoodDishConnection { DayFoodId = Id, FoodId = value.Id, IsMainDish = true });
            }
        }


        [NotMapped]
        public List<Food> SideDishes
        {
            get
            {
                return FoodConnections.Where(x => !x.IsMainDish).Select(x => x.Food).ToList();
            }
            set
            {
                FoodConnections.AddRange(value.Select(x => new DayFoodDishConnection { DayFoodId = Id, FoodId = x.Id, IsMainDish = false }));
            }
        }

        public List<DayFoodDishConnection> FoodConnections { get; set; } = [];

        public double Portions { get; set; } = 1;

        public bool IsVego { get; set; }

        public string CombinedName => string.Join(" + ", Enumerable.Empty<Food>().Concat([MainFood]).Concat(SideDishes).Select(x => x.Name)) +
            (IsVego ? " (vego)" : "");

        public List<Category> GetCategories(AppDbContext dbContext)
        {
            var result = MainFood.Categories.ToHashSet();
            foreach (var cats in SideDishes.Select(x => x.Categories.ToHashSet()))
            {
                foreach (var category in cats)
                {
                    if (!result.Contains(category) && !category.NeedsOnAllSides)
                    {
                        result.Add(category);
                    }
                }

                var catsToRemove = new HashSet<Category>();
                foreach (var category in result)
                {
                    if (category.NeedsOnAllSides && !cats.Contains(category))
                    {
                        catsToRemove.Add(category);
                    }
                }

                result = result.Where(x => !catsToRemove.Contains(x)).ToHashSet();
            }

            if (IsVego)
            {
                var vegoCategory = dbContext.Category.FirstOrDefault(x => x.Name == "Vegetariskt");
                if (vegoCategory != null && !result.Contains(vegoCategory))
                {
                    result.Add(vegoCategory);
                }

                string[] meatCategories = ["Rött kött", "Chark", "Fläsk"];
                result = result.Where(x => !meatCategories.Contains(x.Name)).ToHashSet();
            }

            return [.. result];
        }
    }
}
