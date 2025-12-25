using HomePage.Data;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    public class WordMixLeaderboardModel(AppDbContext dbContext, WordMixResultRepository wordMixResultRepository, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public Dictionary<string, int> TotalWins = [];

        public Dictionary<string, int> MrRobotTies = [];

        public Dictionary<string, double> WinsByParticipation = [];

        public Dictionary<string, Dictionary<string, int>> ScoresAgainstOthers = [];

        public Dictionary<string, int> WinsAgainstOthers = [];

        public Dictionary<string, string> NamesByUsername = [];

        public Dictionary<string, (int wins, int losses)> PersonalResults = [];

        public IActionResult OnGet()
        {
            var allDateGroupings = wordMixResultRepository.GetResultsByDay();
            NamesByUsername = dbContext.UserInfo.ToDictionary(x => x.UserName, x => x.DisplayName);
            Dictionary<string, (double participations, double wins)> participationWins = [];

            if (allDateGroupings.Count == 0)
            {
                return Page();
            }

            foreach (var pair in allDateGroupings)
            {
                var allResults = pair.ToList();

                var humanResults = allResults.Where(x => x.Person != Person.MrRobot.UserName).ToList();
                if (humanResults.Count < 2)
                {
                    continue;
                }

                var mrRobotResult = allResults.FirstOrDefault(x => x.Person == Person.MrRobot.UserName)?.Score;
                var resultsByScore = humanResults.ToDictionary(x => x.Person, x => x.Score);
                var personsAbove = new List<string>();
                foreach (var r in humanResults.OrderByDescending(x => x.Score))
                {
                    if (!TotalWins.ContainsKey(r.Person))
                    {
                        TotalWins[r.Person] = 0;
                    }

                    if (!participationWins.ContainsKey(r.Person))
                    {
                        participationWins[r.Person] = (0, 0);
                    }

                    if (!MrRobotTies.ContainsKey(r.Person))
                    {
                        MrRobotTies[r.Person] = 0;
                    }

                    if (!ScoresAgainstOthers.ContainsKey(r.Person))
                    {
                        ScoresAgainstOthers[r.Person] = [];
                    }

                    var winsByParticipations = participationWins[r.Person];
                    participationWins[r.Person] = (winsByParticipations.participations + 1, winsByParticipations.wins);

                    var crntPerson = r.Person;
                    foreach (var p in personsAbove.Where(p => resultsByScore[p] > r.Score))
                    {
                        var dict = ScoresAgainstOthers[p];
                        if (!dict.TryGetValue(r.Person, out int value))
                        {
                            dict[r.Person] = 1;
                        } else
                        {
                            dict[r.Person] = ++value;
                        }
                    }

                    personsAbove.Add(crntPerson);
                }

                foreach (var r in humanResults.Where(x => x.Score >= mrRobotResult))
                {
                    MrRobotTies[r.Person]++;
                }

                var winnerScore = humanResults.Max(x => x.Score);
                foreach (var winner in humanResults.Where(x => x.Score == winnerScore).Select(x => x.Person))
                {
                    TotalWins[winner]++;
                    participationWins[winner] = (participationWins[winner].participations, participationWins[winner].wins + 1);
                }
            }

            WinsByParticipation = participationWins.ToDictionary(x => x.Key, x => GetWinPercentage(x.Value.participations, x.Value.wins));
            var allParticipants = participationWins.Keys.ToList();
            foreach (var p1 in allParticipants)
            {
                var count = 0;
                foreach (var p2 in allParticipants.Where(x => p1 != x))
                {
                    if (HasBetterScoreThan(p1, p2))
                    {
                        count++;
                    }
                }

                WinsAgainstOthers[p1] = count;
            }

            if (LoggedInPerson != null)
            {
                var username = LoggedInPerson.UserName;
                foreach (var p2 in allParticipants.Where(x => username != x))
                {
                    var wins = WinsAgainstPerson(username, p2);
                    var losses = WinsAgainstPerson(p2, username);
                    PersonalResults.Add(p2, (wins, losses));
                }
            }

            return Page();
        }

        public bool HasBetterScoreThan(string person1, string person2)
        {
            return WinsAgainstPerson(person1, person2) > WinsAgainstPerson(person2, person1);
        }

        public int WinsAgainstPerson(string person1, string person2)
        {
            if (!ScoresAgainstOthers.TryGetValue(person1, out var dict))
            {
                return 0;
            }

            if  (!dict.TryGetValue(person2, out var wins))
            {
                return 0;
            }

            return wins;
        }

        public double GetWinPercentage(double participation, double wins) => Math.Round(wins / participation, 3) * 100;
    }
}
