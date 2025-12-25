using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class Movie
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string Name { get; set; }

        public int Year { get; set; } = 2000;

        [MaxLength(2000)]
        public string? ImageUrl { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(50)]
        public string Owner { get; set; }

        public int JensRanking => Rankings.FirstOrDefault(x => x.Person == Person.Jens.Name)?.Ranking ?? 0;

        public int AnnaRanking => Rankings.FirstOrDefault(x => x.Person == Person.Anna.Name)?.Ranking ?? 0;

        public List<MovieRankning> Rankings { get; set; } = [];

        public List<MovieRankning> GetRankingsWithDefaults()
        {
            var rankings = Rankings.ToList();
            if (!rankings.Any(x => x.Person == Person.Jens.Name) && IsCompleted)
            {
                rankings.Add(new MovieRankning { Person = Person.Jens.Name, Ranking = 0 });
            }

            if (!rankings.Any(x => x.Person == Person.Anna.Name) && IsCompleted)
            {
                rankings.Add(new MovieRankning { Person = Person.Anna.Name, Ranking = 0 });
            }

            return rankings.OrderBy(x => x.Person).ToList();
        }

        public int AverageRanking => JensRanking > 0 && AnnaRanking > 0 ? (JensRanking + AnnaRanking) / 2 : JensRanking + AnnaRanking;

        public string RankingText(MovieRankning ranking, Func<string, string> translateUsername) => translateUsername(ranking.Person) + ": " + (ranking.Ranking > 0 ? string.Join("", Enumerable.Repeat("⭐", ranking.Ranking)) : "-");
    }

    public class MovieRankning
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public required string Person { get; set; }

        public int Ranking { get; set; }

        public Guid MovieId { get; set; }
    }
}
