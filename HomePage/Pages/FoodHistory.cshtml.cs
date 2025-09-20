using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class FoodHistoryModel : PageModel
    {
        public List<(DayFood dayFood, string jensRanking, string annaRanking, string jensNote, string annaNote)> HistoryList { get; set; } = [];
        public void OnGet(string foodId)
        {
            var allFoods = new FoodRepository().GetValues();
            var relevantDayFoods = new DayFoodRepository().GetValues().Values
                .Where(x => x.FoodId == foodId || x.SideDishIds.Contains(foodId))
                .OrderByDescending(x => x.Day);


            var foodRankings = new FoodRankingRepository().GetValues();
            foreach (var history in relevantDayFoods)
            {
                history.Food = allFoods[history.FoodId];
                history.LoadSideDishes(allFoods);
                HistoryList.Add((
                    history, 
                    GetRanking(Person.Jens.Name, history.Day), 
                    GetRanking(Person.Anna.Name, history.Day),
                    GetRankingNote(Person.Jens.Name, history.Day),
                    GetRankingNote(Person.Anna.Name, history.Day)
                    ));
            }

            string GetRankingString(string person, string ranking) => person + ": " + ranking;

            string GetRanking(string person, DateTime date)
            {
                if (foodRankings.TryGetValue(FoodRanking.MakeId(date, person), out var ranking))
                {
                    return GetRankingString(person, ranking.RankingText);
                }

                return GetRankingString(person, "-");
            }

            string GetRankingNote(string person, DateTime date)
            {
                if (foodRankings.TryGetValue(FoodRanking.MakeId(date, person), out var ranking))
                {
                    return ranking.Note;
                }

                return "";
            }
        }
    }
}
