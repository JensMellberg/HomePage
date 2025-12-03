namespace HomePage
{
    public class FlossChore : SinglePersonChore
    {
        public override string Id => "Floss";

        protected override int DaysBetween => 3;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(SettingsTemp settings) => settings.LastFlossTime;

        protected override void SetLastUpdated(SettingsTemp settings, string date)
        {
            settings.LastFlossTime = date;
        }
    }
}
