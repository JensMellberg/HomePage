using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class CachedWikiGamePage
    {
        [Key]
        [MaxLength(2000)]
        public required string Title { get; set; }

        public DateTime LastUsed { get; set; } = DateHelper.DateTimeNow;

        public required string PageContent { get; set; }

        public required List<string> AllowedLinks { get; set; }
    }
}
