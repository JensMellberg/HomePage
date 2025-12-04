using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class RedDay
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public bool IsRed { get; set; }

        [MaxLength(200)]
        public string? DayName { get; set; }
    }
}
