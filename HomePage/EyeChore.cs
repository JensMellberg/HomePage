namespace HomePage
{
    public class EyeChore : SinglePersonChore
    {
        public override string Id => "Eye";

        protected override int DaysBetween => 31;

        protected override string PersonName => Person.Jens.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastEyeTime;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastEyeTime = date;
        }
    }
}
