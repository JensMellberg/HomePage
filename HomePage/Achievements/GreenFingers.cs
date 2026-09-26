namespace HomePage.Achievements
{
    public class GreenFingers : Achievement
    {
        public override string Title => "Gröna fingrar";

        public override string Description => "Få en streak på 20 med att vattna blommor";

        public override int Points => 10;

        public override string Icon => "🌻";

        public override int Id => 5;

        public override bool AdminOnly => true;
    }
}
