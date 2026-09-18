using Microsoft.EntityFrameworkCore;

namespace HomePage.LogoGame
{
    [PrimaryKey(nameof(UserName), nameof(Date))]
    public class LogotypeResult
    {
        public required string UserName { get; set; }

        public DateTime Date { get; set; }

        public required string Answer { get; set; }

        public int HintsUsed { get; set; }

        public bool IsCorrect { get; set; }
    }
}
