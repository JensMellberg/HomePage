using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomePage.Pages
{
    public class FoodHistoryModel(AppDbContext dbContext, DayFoodRepository dayFoodRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Food Food;
        public List<(DayFood dayFood, string jensRanking, string annaRanking, string jensNote, string annaNote)> HistoryList { get; set; } = [];
        public void OnGet(Guid foodId)
        {
            var relevantDayFoods = dayFoodRepository.GetPopulatedDayFood()
                .Where(x => x.FoodConnections.Any(x => x.FoodId == foodId))
                .OrderByDescending(x => x.Date)
                .ToList();

            Food = relevantDayFoods.Count > 0 ? relevantDayFoods[0].MainFood : 
                dbContext.Food.Include(x => x.FoodIngredients).ThenInclude(x => x.Ingredient).Include(x => x.Categories).AsSplitQuery().Single(x => x.Id == foodId);

            var foodRankings = dbContext.FoodRanking.ToList();
            foreach (var history in relevantDayFoods)
            {
                HistoryList.Add((
                    history, 
                    GetRanking(Person.Jens.Name, history.Date), 
                    GetRanking(Person.Anna.Name, history.Date),
                    GetRankingNote(Person.Jens.Name, history.Date),
                    GetRankingNote(Person.Anna.Name, history.Date)
                    ));
            }

            string GetRankingString(string person, string ranking) => person + ": " + ranking;

            string GetRanking(string person, DateTime date)
            {
                var ranking = foodRankings.FirstOrDefault(x => x.Date == date && x.Person == person);
                return GetRankingString(person, ranking?.RankingText ?? "-");
            }

            string GetRankingNote(string person, DateTime date)
            {
                var ranking = foodRankings.FirstOrDefault(x => x.Date == date && x.Person == person);
                return ranking?.Note ?? "";
            }
        }
    }
}
