using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class CachedWikiGameLinks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PageId { get; set; }

        [MaxLength(2000)]
        public required string Title { get; set; }

        public DateTime CacheDate { get; set; }

        public List<CachedWikipediaLink>? IncomingLinks { get; set; }

        public List<CachedWikipediaLink>? OutgoingLinks { get; set; }
    }

    public record CachedWikipediaLink(long Id, string Title);
}
