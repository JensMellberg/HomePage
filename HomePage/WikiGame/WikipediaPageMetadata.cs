namespace HomePage.WikiGame
{
    public record WikipediaPageMetadata(long Id, string Title)
    {
        public string NormalizedTitle => Title.Replace(' ', '_');
    }

    public record WikipediaPageMetadataWithSummary(long Id, string Title, string Summary) : WikipediaPageMetadata(Id, Title);
}
