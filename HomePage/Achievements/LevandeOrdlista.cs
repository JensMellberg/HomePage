namespace HomePage.Achievements
{
    public class LevandeOrdlista : Achievement
    {
        public override string Title => "Levande ordlista";

        public override string Description => "Få lika mycket poäng som Mr. Robot i ordmix";

        public override int Points => 15;

        public override string Icon => "📖";

        public override int Id => 3;

        public override bool AdminOnly => false;
    }
}
