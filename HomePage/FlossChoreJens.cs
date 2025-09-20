namespace HomePage
{
    public class FlossChoreJens : SinglePersonChore
    {
        public override string Id => "FlossJens";

        protected override int DaysBetween => 3;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastFlossTimeJens;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastFlossTimeJens = date;
        }
    }
}
