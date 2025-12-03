namespace HomePage
{
    public class SinkChore : SinglePersonChore
    {
        public override string Id => "Sink";

        protected override int DaysBetween => 93;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(SettingsTemp settings) => settings.LastSinkTime;

        protected override void SetLastUpdated(SettingsTemp settings, string date)
        {
            settings.LastSinkTime = date;
        }
    }
}
