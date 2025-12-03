namespace HomePage
{
    public class EyeChore : SinglePersonChore
    {
        public override string Id => "Eye";

        protected override int DaysBetween => 31;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(SettingsTemp settings) => settings.LastEyeTime;

        protected override void SetLastUpdated(SettingsTemp settings, string date)
        {
            settings.LastEyeTime = date;
        }
    }
}
