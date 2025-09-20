using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class WordMixStatsModel : PageModel
    {
        public int JensScore { get; set; }

        public int AnnaScore { get; set; }

        public string JensBoard { get; set; }

        public string AnnaBoard { get; set; }

        public List<(int jensScore, int annaScore, DateTime date)> History { get; set; } = [];

        public string ConvertScore(int score) => score == 0 ? "-" : score.ToString();

        public int WaitingForApproval { get; set; }

        public IActionResult OnGet()
        {
            this.TryLogIn();
            var allDateGroupings = new WordMixResultRepository()
                .GetValues().Values
                .GroupBy(x => x.Day)
                .OrderByDescending(x => DateHelper.FromKey(x.Key))
                .ToList();

            if (allDateGroupings.Count == 0)
            {
                return Page();
            }

            var loggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name!;
            if (allDateGroupings.First().Key == DateHelper.ToKey(DateHelper.DateNow))
            {
                foreach (var entry in allDateGroupings.First())
                {
                    if (entry.Person == Person.Jens.Name)
                    {
                        JensScore = entry.Score;
                        if (loggedInPerson == Person.Jens.Name)
                        {
                            JensBoard = entry.Board;
                        }
                    } 
                    else
                    {
                        AnnaScore = entry.Score;
                        if (loggedInPerson == Person.Anna.Name)
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
                foreach (var entry in dateGrouping)
                {
                    if (entry.Person == Person.Jens.Name)
                    {
                        jensScore = entry.Score;
                    }
                    else
                    {
                        annaScore = entry.Score;
                    }
                }

                History.Add((jensScore, annaScore, DateHelper.FromKey(dateGrouping.Key)));
            }

            WaitingForApproval = new ExtraWordRepository().GetValues().Values
                    .Count(x => loggedInPerson == Person.Jens.Name && !x.JensApproved || loggedInPerson == Person.Anna.Name && !x.AnnaApproved);

            return Page();
        }
    }
}
