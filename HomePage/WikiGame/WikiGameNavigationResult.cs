namespace HomePage.WikiGame
{
    public class WikiGameNavigationResult
    {
        public required string Html { get; set; }

        public int Steps { get; set; }

        public bool IsWin { get; set; }

        public bool CanGoBack { get; set; }
    }
}
