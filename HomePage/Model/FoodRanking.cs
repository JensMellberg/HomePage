using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class FoodRanking
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string RankingText => string.IsNullOrEmpty(FoodId) ? "-" : Ranking.ToString();

        public string RankingTextWithNote => RankingText + (string.IsNullOrEmpty(Note) ? "" : " " + Note);

        [SaveProperty]
        [MaxLength(2000)]
        public string? Note { get; set; }

        public DateTime Date { get; set; }

        [SaveProperty]
        [MaxLength(50)]
        public string Person { get; set; }

        [SaveProperty]
        public string FoodId { get; set; }

        [SaveProperty]
        public int Ranking { get; set; }
    }
}
