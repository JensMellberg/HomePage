namespace HomePage.Achievements
{
    public class WikiNerd : Achievement
    {
        public override string Title => "Wikinörd";

        public override string Description => "Hitta dagens wikisida med lika få klick som Mr. Robot";

        public override int Points => 15;

        public override string Icon => "🌐";

        public override int Id => 4;

        public override bool AdminOnly => false;
    }
}
