using System.Security.Cryptography.X509Certificates;

namespace HomePage
{
    public class Movie : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public int Year { get; set; } = 2000;

        [SaveProperty]
        public string ImageUrl { get; set; }

        [SaveProperty]
        public bool IsCompleted { get; set; }

        [SaveProperty]
        public string CompletedDate { get; set; }

        [SaveProperty]
        public int JensRanking { get; set; }

        [SaveProperty]
        public int AnnaRanking { get; set; }

        public int AverageRanking => JensRanking > 0 && AnnaRanking > 0 ? (JensRanking + AnnaRanking) / 2 : JensRanking + AnnaRanking;

        public string RankingText(Person person) => MakeRanking(person, person == Person.Jens ? JensRanking : AnnaRanking);

        public string MakeRanking(Person person, int ranking) => person.Name + ": " + (ranking > 0 ? string.Join("", Enumerable.Repeat("⭐", ranking)) : "-");
    }
}
