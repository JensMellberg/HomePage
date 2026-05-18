using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class WikiGameStart
    {
        [Key]
        public DateTime Date { get; set; } = DateHelper.DateNow;

        [MaxLength(2000)]
        public required string Title { get; set; }

        [MaxLength(2000)]
        public required string GoalTitle { get; set; }

        public required string Summary { get; set; }

        [MaxLength(10)]
        public required string Language { get; set; }
    }
}
