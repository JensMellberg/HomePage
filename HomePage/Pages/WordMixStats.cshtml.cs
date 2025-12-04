using HomePage.Data;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class WordMixStatsModel(AppDbContext dbContext, WordMixResultRepository wordMixResultRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public int JensScore { get; set; }

        public int AnnaScore { get; set; }

        public string JensBoard { get; set; }

        public string AnnaBoard { get; set; }

        public List<(int jensScore, int annaScore, DateTime date, Guid jensKey, Guid annaKey)> History { get; set; } = [];

        public string ConvertScore(int score) => score == 0 ? "-" : score.ToString();

        public int WaitingForApproval { get; set; }

        public IActionResult OnGet()
        {
            var allDateGroupings = dbContext.WordMixResult
                .ToList()
                .GroupBy(x => x.Date)
                .OrderByDescending(x => x.Key)
                .ToList();

            if (allDateGroupings.Count == 0)
            {
                return Page();
            }

            var loggedInPersonName = LoggedInPerson?.UserName;
            if (allDateGroupings.First().Key == DateHelper.DateTimeNow.Date)
            {
                foreach (var entry in allDateGroupings.First())
                {
                    if (entry.Person == Person.Jens.Name)
                    {
                        JensScore = entry.Score;
                        if (loggedInPersonName == Person.Jens.Name)
                        {
                            JensBoard = entry.Board;
                        }
                    } 
                    else
                    {
                        AnnaScore = entry.Score;
                        if (loggedInPersonName == Person.Anna.Name)
                        {
                            AnnaBoard = entry.Board;
                        }
                    }
                }

                allDateGroupings.RemoveAt(0);
            }

            foreach (var dateGrouping in allDateGroupings)
            {
                var jensScore = 0;
                var annaScore = 0;
                Guid jensKey = Guid.Empty;
                Guid annaKey = Guid.Empty;
                foreach (var entry in dateGrouping)
                {
                    if (entry.Person == Person.Jens.Name)
                    {
                        jensScore = entry.Score;
                        jensKey = entry.Id;
                    }
                    else
                    {
                        annaScore = entry.Score;
                        annaKey = entry.Id;
                    }
                }

                History.Add((jensScore, annaScore, dateGrouping.Key, jensKey, annaKey ));
            }

            WaitingForApproval = loggedInPersonName != null
                ? dbContext.ExtraWord
                    .Count(x => loggedInPersonName == Person.Jens.Name && !x.JensApproved || loggedInPersonName == Person.Anna.Name && !x.AnnaApproved)
                : 0;

            return Page();
        }
    }
}
