namespace HomePage.Model
{
    public class ChoreStreak
    {
        public required string ChoreId { get; set; }

        public ChoreModel Chore { get; set; }

        public int Streak { get; set; }

        public string? Person { get; set; }
    }
}
