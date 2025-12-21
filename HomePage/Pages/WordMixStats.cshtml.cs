using HomePage.Data;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static HomePage.WordMixResultValidator;

namespace HomePage.Pages
{
    public class WordMixStatsModel(AppDbContext dbContext, WordMixResultRepository wordMixResultRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public WordMixResultClientModel? JensScore { get; set; }

        public WordMixResultClientModel? AnnaScore { get; set; }

        public List<WordMixResultClientModel> OtherScores { get; set; } = [];

        public List<(WordMixResultClientModel? jensScore, WordMixResultClientModel? annaScore, List<WordMixResultClientModel> others, DateTime date)> History { get; set; } = [];

        public string ConvertScore(int? score) => !score.HasValue || (score.Value == 0) ? "-" : score.ToString();

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

            var accounts = dbContext.UserInfo.ToDictionary(x => x.UserName, x => x);
            var loggedInPersonName = LoggedInPerson?.UserName;
            if (allDateGroupings.First().Key == DateHelper.DateNow)
            {
                foreach (var entry in allDateGroupings.First())
                {
                    var clientModel = new WordMixResultClientModel
                    {
                        Score = entry.Score,
                        Person = accounts[entry.Person].DisplayName.Truncate(10),
                        Key = loggedInPersonName == entry.Person ? entry.Board : null
                    };
                    if (entry.Person == Person.Jens.Name)
                    {
                        JensScore = clientModel;
                    }
                    else if (entry.Person == Person.Anna.Name)
                    {
                        AnnaScore = clientModel;
                    }
                    else
                    {
                        OtherScores.Add(clientModel);
                    }
                }

                allDateGroupings.RemoveAt(0);
            }

            foreach (var dateGrouping in allDateGroupings)
            {
                WordMixResultClientModel? jensScore = null;
                WordMixResultClientModel? annaScore = null;
                var otherScores = new List<WordMixResultClientModel>();
                foreach (var entry in dateGrouping)
                {
                    var clientModel = new WordMixResultClientModel
                    {
                        Score = entry.Score,
                        Person = accounts[entry.Person].DisplayName.Truncate(10),
                        Key = entry.Id.ToString()
                    };
                    if (entry.Person == Person.Jens.Name)
                    {
                        jensScore = clientModel;
                    }
                    else if (entry.Person == Person.Anna.Name)
                    {
                        annaScore = clientModel;
                    }
                    else
                    {
                        otherScores.Add(clientModel);
                    }
                }

                History.Add((jensScore, annaScore, otherScores, dateGrouping.Key ));
            }

            WaitingForApproval = loggedInPersonName != null
                ? dbContext.ExtraWord
                    .Count(x => loggedInPersonName == Person.Jens.Name && !x.JensApproved || loggedInPersonName == Person.Anna.Name && !x.AnnaApproved)
                : 0;



            /*var letterString = new CurrentWordMixRepository(dbContext).GetCurrent().Letters;
            var letterList = new List<Letter>();
            for (var i = 0; i < letterString.Length; i += 2)
            {
                letterList.Add(new Letter { Character = letterString[i], Score = int.Parse(letterString[i + 1].ToString()) });
            }

            var solver = new WordMixCalculator(letterList, dbContext);
            var solution = solver.CalculateBestBoard();*/

            return Page();
        }

        public class WordMixResultClientModel
        {
            public required string Person { get; set; }

            public string? Key { get; set; }

            public int Score { get; set; }
        }
    }
}
