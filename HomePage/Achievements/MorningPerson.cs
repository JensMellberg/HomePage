namespace HomePage.Achievements
{
    public class MorningPerson : Achievement
    {
        public override string Title => "Morgonpigg";

        public override string Description => "Vattna blommorna innan klockan 8 på morgonen";

        public override int Points => 10;

        public override string Icon => "🥱";

        public override int Id => 10;

        public override bool AdminOnly => true;
    }
}
