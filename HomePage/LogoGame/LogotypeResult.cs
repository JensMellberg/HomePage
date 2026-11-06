using Microsoft.EntityFrameworkCore;

namespace HomePage.LogoGame
{
    [PrimaryKey(nameof(UserName), nameof(Date))]
    public class LogotypeResult
    {
        public required string UserName { get; set; }

        public required DateTime Date { get; set; }

        public required string Answer { get; set; }

        public int HintsUsed { get; set; }

        public bool IsCorrect { get; set; }

        public int Guesses { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public bool WasFastResult => FinishedAt.HasValue && FinishedAt < CreatedAt.AddSeconds(15);
    }
}
