namespace HomePage
{
    public class FlossChoreJens : SinglePersonChore
    {
        public override string Id => "FlossJens";

        protected override int DaysBetween => 3;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(SettingsTemp settings) => settings.LastFlossTimeJens;

        protected override void SetLastUpdated(SettingsTemp settings, string date)
        {
            settings.LastFlossTimeJens = date;
        }
    }
}
