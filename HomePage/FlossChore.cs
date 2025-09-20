namespace HomePage
{
    public class FlossChore : SinglePersonChore
    {
        public override string Id => "Floss";

        protected override int DaysBetween => 3;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastFlossTime;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastFlossTime = date;
        }
    }
}
