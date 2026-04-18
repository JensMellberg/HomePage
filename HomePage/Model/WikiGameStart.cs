using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class WikiGameStart
    {
        [Key]
        public DateTime Date { get; set; } = DateHelper.DateNow;

        [MaxLength(2000)]
        public required string Title { get; set; }
    }
}
