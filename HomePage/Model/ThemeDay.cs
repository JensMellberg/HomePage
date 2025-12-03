using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ThemeDay
    {
        [Key]
        [MaxLength(50)]
        public string Key { get; set; }

        public DateTime ThemeDate { get; set; }

        [MaxLength(200)]
        public string DayName { get; set; }
    }
}
