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

        public int JensRanking { get; set; }

        public int AnnaRanking { get; set; }

        public int AverageRanking => JensRanking > 0 && AnnaRanking > 0 ? (JensRanking + AnnaRanking) / 2 : JensRanking + AnnaRanking;

        public string RankingText(Person person) => MakeRanking(person, person == Person.Jens ? JensRanking : AnnaRanking);

        public string MakeRanking(Person person, int ranking) => person.Name + ": " + (ranking > 0 ? string.Join("", Enumerable.Repeat("⭐", ranking)) : "-");
    }
}
