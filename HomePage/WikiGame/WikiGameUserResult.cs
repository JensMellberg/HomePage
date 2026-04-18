namespace HomePage.WikiGame
{
    public class WikiGameUserResult
    {
        public required string UserName { get; set; }

        public int Steps { get; set; }

        public required List<string> PathTaken { get; set; }
    }
}
