namespace HomePage.LogoGame
{
    public class LogotypeDayResult
    {
        public required LogotypeMetadata LogotypeMetadata { get; set; }

        public required List<LogotypeResult> Results { get; set; }

        public DateTime Date { get; set; }
    }
}
