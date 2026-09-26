namespace HomePage.Achievements
{
    public class QuickClick : Achievement
    {
        public override string Title => "Snabb som fan";

        public override string Description => "Gissa rätt på dagens logotyp inom 10 sekunder efter att du öppnat sidan";

        public override int Points => 30;

        public override string Icon => "⚡";

        public override int Id => 7;

        public override bool AdminOnly => false;
    }
}
