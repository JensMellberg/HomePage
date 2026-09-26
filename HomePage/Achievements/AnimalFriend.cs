namespace HomePage.Achievements
{
    public class AnimalFriend : Achievement
    {
        public override string Title => "Djurvän";

        public override string Description => "Ät en vegetarisk rätt 10 veckor i rad";

        public override int Points => 20;

        public override string Icon => "🐇";

        public override int Id => 6;

        public override bool AdminOnly => true;
    }
}
