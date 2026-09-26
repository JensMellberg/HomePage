namespace HomePage.Achievements
{
    public class Reklamskadad : Achievement
    {
        public override string Title => "Reklamskadad";

        public override string Description => "Gissa rätt på dagens logotyp på första försöket 5 dagar i rad utan ledtrådar";

        public override int Points => 20;

        public override string Icon => "🤑";

        public override int Id => 8;

        public override bool AdminOnly => false;
    }
}
