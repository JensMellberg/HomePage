using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class CachedWikiGameLinks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PageId { get; set; }

        public DateTime CacheDate { get; set; }

        public List<long>? IncomingLinks { get; set; }

        public List<long>? OutgoingLinks { get; set; }
    }
}
