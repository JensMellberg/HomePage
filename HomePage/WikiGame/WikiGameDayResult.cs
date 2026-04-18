namespace HomePage.WikiGame
{
    public class WikiGameDayResult
    {
        public DateTime Date { get; set; }

        public required List<WikiGameUserResult> Results { get; set; }
    }
}
