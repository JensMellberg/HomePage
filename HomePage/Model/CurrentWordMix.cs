using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class CurrentWordMix
    {
        [Key]
        [MaxLength(50)]
        public string Key { get; set; } = "Single";

        public DateTime CreatedAt { get; set; }

        [MaxLength(200)]
        public string Letters { get; set; }

        public bool ShouldRecreate => CreatedAt.Date < DateHelper.DateNow;
    }
}
