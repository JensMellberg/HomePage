using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class WikiGameNavigation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateHelper.DateNow;

        [MaxLength(100)]
        public required string UserName { get; set; }

        [MaxLength(2000)]
        public required string Title { get; set; }

        public int Step { get; set; }

        public Guid? BackId { get; set; }

        public bool IsWinNavigation => BackId == null;
    }
}
