namespace HomePage
{
    public class BedSheetChore : SinglePersonChore
    {
        public override string Id => "Bed";

        protected override int DaysBetween => 14;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastBedTime;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastBedTime = date;
        }
    }
}
