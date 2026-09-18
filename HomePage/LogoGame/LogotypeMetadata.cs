namespace HomePage.LogoGame
{
    public class LogotypeMetadata
    {
        public required string[] Answers { get; set; }

        public required string LogoUrl { get; set; }

        public string? OriginalLogoUrl { get; set; }

        public int HintsAvailable { get; set; }

        public string Id { get; set; } = "";

        public string GetOriginalLogo => OriginalLogoUrl ?? LogoUrl;
    }
}
