namespace HomePage.Achievements
{
    public class Nattsurfare : Achievement
    {
        public override string Title => "Nattsurfare";

        public override string Description => "Klara wiki race't innan kl 1 på natten.";

        public override int Points => 15;

        public override string Icon => "🦉";

        public override int Id => 1;

        public override bool AdminOnly => false;
    }
}
