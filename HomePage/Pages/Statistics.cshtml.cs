using HomePage.Data;
using HomePage.Model;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    public enum StatisticType
    {
        Amount,
        Category,
        AverageRanking,
        SpendingPerGroup,
        EatenPerDay,
        RankingPerCategory
    }

    public class GridDataCell
    {
        public string Text { get; set; }

        public List<GridDataCell[]> DrillDown { get; set; }
    }

    public class StatisticsModel(AppDbContext dbContext, SpendingGroupRepository spendingGroupRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public (StatisticType, string)[] StatisticOptions = [
            (StatisticType.Amount, "Antal ätna"),
            (StatisticType.Category, "Kategorier ätna"),
            (StatisticType.AverageRanking, "Snittbetyg"),
            (StatisticType.SpendingPerGroup, "Spenderande"),
            (StatisticType.EatenPerDay, "Maträtt per dag"),
            (StatisticType.RankingPerCategory, "Betyg per kategori")
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
            var relevantFoods = dbContext.DayFood
                .Include(x => x.FoodConnections)
                .Where(x => x.Date >= from)
                .Where(x => x.Date <= to)
                .ToList();
            var allFoods = dbContext.Food
                .Include(x => x.Categories)
                .Include(x => x.FoodIngredients)
                .ToDictionary(x => x.Id, x => x);

            if (CurrentType == StatisticType.Amount)
            {
                var groupPairs = relevantFoods.GroupBy(x => x.MainFoodId).Select(x => (x.Key, x.Count())).OrderByDescending(x => x.Item2);
                GridData = groupPairs.Select(x => (GridDataCell[])[
                    new GridDataCell { Text = allFoods[x.Key].Name },
                    new GridDataCell { Text = x.Item2.ToString() }
                ]).ToList();
            }
            else if (CurrentType == StatisticType.EatenPerDay)
            {
                var foodRankings = dbContext.FoodRanking.ToList();
                GridData = relevantFoods.OrderByDescending(x => x.Date).Select(x => (GridDataCell[])[
                    new GridDataCell { Text = DateHelper.ToNumberedDateString(x.Date) },
                    new GridDataCell { Text = x.CombinedName },
                    new GridDataCell { Text = $"{Person.Jens.Name}: {GetRanking(Person.Jens.Name, x.Date)}" },
                    new GridDataCell { Text = $"{Person.Anna.Name}: {GetRanking(Person.Anna.Name, x.Date)}" },
                ]).ToList();

                string GetRanking(string person, DateTime date) {
                    var ranking = foodRankings.FirstOrDefault(x => x.Date == date && x.Person == person);
                    return ranking?.RankingText ?? "-";
                }
            }
            else if (CurrentType == StatisticType.Category)
            {
                var allCategories = dbContext.Category.ToList();
                var categoriesForDayFood = new Dictionary<Guid, List<Category>>();
                foreach (var dayFood in relevantFoods)
                {
                    categoriesForDayFood.Add(dayFood.Id, dayFood.GetCategories(dbContext));
                }

                GridData = [];
                foreach (var cat in allCategories)
                {
                    var foods = relevantFoods.Where(x => categoriesForDayFood[x.Id].Contains(cat));
                    var drilldownGrid = foods.Select(x => (GridDataCell[])[new() { Text = x.CombinedName }]).ToList();
                    GridData.Add([new() { Text = cat.Name }, new() { Text = drilldownGrid.Count.ToString() }, new() { DrillDown = drilldownGrid }]);
                }

                GridData = GridData.OrderByDescending(x => int.Parse(x[1].Text)).ToList();
            }
            else if (CurrentType == StatisticType.AverageRanking)
            {
                var rankings = dbContext.FoodRanking.Where(x => x.Date >= from && x.Date <= to).ToList();
                Utils.CalculateAverages(rankings, out var jensAverage, out var annaAverage, out var totalAverage);
                GridData = [
                    [new() { Text = "Jens"}, new() { Text = jensAverage.ToString()}],
                    [new() { Text = "Anna"}, new() { Text = annaAverage.ToString()}],
                    [new() { Text = "Totalt"}, new() { Text = totalAverage.ToString()}],
                ];
            }
            else if (CurrentType == StatisticType.RankingPerCategory)
            {
                var allCategories = dbContext.Category.ToDictionary(x => x.Key, x => x);
                var categoryRankings = allCategories.ToDictionary(x => x.Key, x => new List<FoodRanking>());
                var rankings = dbContext.FoodRanking.Where(x => x.Date >= from && x.Date <= to).ToList();

                foreach (var v in relevantFoods)
                {
                    var dayRankings = rankings.Where(x => x.Date == v.Date);
                    v.MainFood = allFoods[v.MainFoodId];
                    var categories = v.GetCategories(dbContext);
                    foreach (var c in categories)
                    {
                        categoryRankings[c.Key].AddRange(dayRankings);
                    }
                }

                GridData = [];
                foreach (var c in categoryRankings)
                {
                    Utils.CalculateAverages(c.Value, out var jensAverage, out var annaAverage, out var totalAverage);
                    GridData.Add([
                        new() { Text = allCategories[c.Key].Name },
                        new() { Text = "jens: " + Utils.GetTextOrNothing(jensAverage) },
                        new() { Text = "anna: " + Utils.GetTextOrNothing(annaAverage) },
                        new() { Text = "totalt: " + Utils.GetTextOrNothing(totalAverage) }]);
                }

                GridData = GridData.OrderByDescending(x => GetValueFromCell(x[3])).ToList();

                double GetValueFromCell(GridDataCell cell)
                {
                    var val = cell.Text.Substring("totalt: ".Length);
                    return val == "-" ? 0 : double.Parse(val);
                }
            }
            else if (CurrentType == StatisticType.SpendingPerGroup)
            {
                var itemsInRange = dbContext.SpendingItem.Where(x => x.TransactionDate >= from && x.TransactionDate <= to).ToList();
                var allGroups = spendingGroupRepository.GetAllSpendingGroups("Both", from, to);
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
