namespace HomePage
{
    public class SinkChore : SinglePersonChore
    {
        public override string Id => "Sink";

        protected override int DaysBetween => 93;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastSinkTime;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastSinkTime = date;
        }
    }
}
