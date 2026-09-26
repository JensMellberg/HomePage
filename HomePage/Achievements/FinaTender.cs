namespace HomePage.Achievements
{
    public class FinaTender : Achievement
    {
        public override string Title => "Tina tänder";

        public override string Description => "Få en streak på 50 med tandtrådning";

        public override int Points => 20;

        public override string Icon => "🦷";

        public override int Id => 2;

        public override bool AdminOnly => true;
    }
}
