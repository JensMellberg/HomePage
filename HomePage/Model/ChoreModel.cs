using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ChoreModel
    {
        [Key]
        [MaxLength(100)]
        public required string Id { get; set; }

        public DateTime LastUpdated { get; set; }

        public required List<ChoreStreak> Streaks { get; set; }

        public string? LastUpdatedPerson { get; set; }
    }
}
