using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class OldGoodiesModel(SignInRepository signInRepository, DayFoodRepository dayFoodRepository, AppDbContext dbContext, DatabaseLogger logger)
        : BasePage(signInRepository)
    {
        private const double DefaultMinimumScore = 6.1;
        public List<(Food food, DateTime lastEaten, double score)> SortedFood { get; set; } = [];
        public double MinimumScore { get; set; }

        public IActionResult OnGet(double? minimumScore)
        {
            MinimumScore = minimumScore ?? DefaultMinimumScore;
            var allFoods = dayFoodRepository.GetPopulatedDayFood();
            var alreadyAdded = new HashSet<Guid>();
            var excluded = dbContext.ExcludedFromGoodies.Select(x => x.FoodId).Distinct().ToHashSet();
            var allRankings = dbContext.FoodRanking.ToList();

            foreach (var dayFood in allFoods.OrderByDescending(x => x.Date))
            {
                if (excluded.Contains(dayFood.MainFoodId) || !alreadyAdded.Add(dayFood.MainFoodId))
                {
                    continue;
                }

                var relevantRankings = allRankings.Where(x => Guid.Parse(x.FoodId) == dayFood.MainFoodId);
                Utils.CalculateAverages(relevantRankings, out var jensAverage, out var annaAverage, out var score);

                if (score >= MinimumScore && jensAverage > MinimumScore - 1 && annaAverage > MinimumScore - 1)
                {
                    SortedFood.Add((dayFood.MainFood, dayFood.Date, Math.Round(score, 1)));
                }
            }

            SortedFood.Reverse();
            return Page();
        }

        public IActionResult OnPost(Guid foodId, string minimumScore)
        {
            var redirectResult = GetPotentialClientRedirectResult(true, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            dbContext.ExcludedFromGoodies.Add(new ExcludedGoodie { FoodId = foodId });
            dbContext.SaveChanges();

            var food = dbContext.Food.Find(foodId);
            logger.Information($"{LoggedInPerson?.UserName} excluded {food?.Name} from old goodies.", LoggedInPerson?.UserName);
            return Utils.CreateRedirectClientResult("OldGoodies?minimumScore=" + minimumScore);
        }
    }
}
