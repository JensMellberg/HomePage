using System.ComponentModel.DataAnnotations;

namespace HomePage.LogoGame
{
    public class DayLogo
    {
        [Key]
        public required DateTime Date { get; set; }

        [MaxLength(100)]
        public required string LogoMetadataPath { get; set; }
    }
}
