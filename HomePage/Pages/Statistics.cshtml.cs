using System.Linq;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public enum StatisticType
    {
        Amount,
        Category,
        AverageRanking,
        SpendingPerGroup,
        EatenPerDay
    }

    public class GridDataCell
    {
        public string Text { get; set; }

        public List<GridDataCell[]> DrillDown { get; set; }
    }

    public class StatisticsModel : PageModel
    {
        public (StatisticType, string)[] StatisticOptions = [
            (StatisticType.Amount, "Antal ätna"),
            (StatisticType.Category, "Kategorier ätna"),
            (StatisticType.AverageRanking, "Snittbetyg"),
            (StatisticType.SpendingPerGroup, "Spenderande"),
            (StatisticType.EatenPerDay, "Maträtt per dag")
        ];

        public List<GridDataCell[]> GridData { get; set; }

        public StatisticType CurrentType { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public void OnGet(string fromDate, string toDate, string type)
        {
            Enum.TryParse(typeof(StatisticType), type, out var parsedType);
            if (parsedType is StatisticType correctlyParsed)
            {
                CurrentType = correctlyParsed;
            }
            else
            {
                CurrentType = StatisticType.Amount;
            }

            var from = string.IsNullOrEmpty(fromDate) ? DateTime.MinValue : DateHelper.FromKey(fromDate);
            var to = string.IsNullOrEmpty(toDate) ? DateTime.MaxValue : DateHelper.FromKey(toDate);
            FromDate = fromDate;
            ToDate = toDate;
            var relevantFoods = new DayFoodRepository().GetValues()
                .Where(x => DateHelper.FromKey(x.Key) >= from)
                .Where(x => DateHelper.FromKey(x.Key) <= to)
                .Select(x => x.Value);
            var allFoods = new FoodRepository().GetValues();

            if (CurrentType == StatisticType.Amount)
            {
                var groupPairs = relevantFoods.GroupBy(x => x.FoodId).Select(x => (x.Key, x.Count())).OrderByDescending(x => x.Item2);
                GridData = groupPairs.Select(x => (GridDataCell[])[
                    new GridDataCell { Text = allFoods[x.Key].Name },
                    new GridDataCell { Text = x.Item2.ToString() }
                ]).ToList();
            }
            else if (CurrentType == StatisticType.EatenPerDay)
            {
                var foodRankings = new FoodRankingRepository().GetValues();
                foreach (var relevantFood in relevantFoods)
                {
                    relevantFood.Food = allFoods[relevantFood.FoodId];
                    relevantFood.LoadSideDishes(allFoods);
                }

                GridData = relevantFoods.OrderBy(x => x.Day).Select(x => (GridDataCell[])[
                    new GridDataCell { Text = DateHelper.ToNumberedDateString(x.Day) },
                    new GridDataCell { Text = x.CombinedName },
                    new GridDataCell { Text = $"{Person.Jens.Name}: {GetRanking(Person.Jens.Name, x.Day)}" },
                    new GridDataCell { Text = $"{Person.Anna.Name}: {GetRanking(Person.Anna.Name, x.Day)}" },
                ]).ToList();

                string GetRanking(string person, DateTime date) {
                    if (foodRankings.TryGetValue(FoodRanking.MakeId(date, person), out var ranking))
                    {
                        return ranking.RankingText;
                    }

                    return "-";
                }
            }
            else if (CurrentType == StatisticType.Category)
            {
                var allCategories = new CategoryRepository().GetValues();
                var categoriesForDayFood = new Dictionary<string, List<Category>>();
                foreach (var dayFood in relevantFoods)
                {
                    dayFood.Food = allFoods[dayFood.FoodId];
                    dayFood.LoadSideDishes(allFoods);
                    categoriesForDayFood.Add(dayFood.Key, dayFood.GetCategories(allCategories));
                }

                GridData = [];
                foreach (var cat in allCategories.Values)
                {
                    var foods = relevantFoods.Where(x => categoriesForDayFood[x.Key].Contains(cat));
                    var drilldownGrid = foods.Select(x => (GridDataCell[])[new() { Text = x.CombinedName }]).ToList();
                    GridData.Add([new() { Text = cat.Name }, new() { Text = drilldownGrid.Count.ToString() }, new() { DrillDown = drilldownGrid }]);
                }

                GridData = GridData.OrderByDescending(x => int.Parse(x[1].Text)).ToList();
            }
            else if (CurrentType == StatisticType.AverageRanking)
            {
                var rankings = new FoodRankingRepository().GetValues().Values
                    .Where(x => DateHelper.FromKey(x.Day) >= from)
                    .Where(x => DateHelper.FromKey(x.Day) <= to);
                Utils.CalculateAverages(rankings, out var jensAverage, out var annaAverage, out var totalAverage);
                GridData = [
                    [new() { Text = "Jens"}, new() { Text = jensAverage.ToString()}],
                    [new() { Text = "Anna"}, new() { Text = annaAverage.ToString()}],
                    [new() { Text = "Totalt"}, new() { Text = totalAverage.ToString()}],
                ];
            }
            else if (CurrentType == StatisticType.SpendingPerGroup)
            {
                var allItems = new SpendingItemRepository().GetValues().Values;
                foreach (var item in allItems)
                {
                    item.ConvertedDate = DateHelper.FromKey(item.Date);
                }

                var itemsInRange = allItems.Where(x => x.ConvertedDate >= from && x.ConvertedDate <= to);
                var allGroups = new SpendingGroupRepository().GetAllSpendingGroups("Both", from, to);
                var sortedItems = SpendingModel.SortIntoGroups(itemsInRange, allGroups);
                GridData = [];
                foreach (var monthGroup in sortedItems.Values.Where(x => x.Total < 0 ))
                {
                    GridData.Add([new() { Text = monthGroup.Name }, new() { Text = (monthGroup.Total * -1).ToString() }]);
                }

                GridData = GridData.OrderByDescending(x => int.Parse(x[1].Text)).ToList();
            }
        }
    }
}
