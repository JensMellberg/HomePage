using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class WikiGameSuggestion
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime DateAdded { get; set; } = DateHelper.DateNow;

        [MaxLength(100)]
        public required string Creator { get; set; }

        [MaxLength(2000)]
        public required string Title { get; set; }

        public int Votes { get; set; }

        [MaxLength(10)]
        public required string Language { get; set; }

        public List<string> Voters { get; set; } = [];
    }
}
