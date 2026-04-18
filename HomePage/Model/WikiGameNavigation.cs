using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class WikiGameNavigation
    {
        public DateTime Date { get; set; } = DateHelper.DateNow;

        [MaxLength(100)]
        public required string UserName { get; set; }

        [MaxLength(2000)]
        public required string Title { get; set; }

        public int Step { get; set; }
    }
}
