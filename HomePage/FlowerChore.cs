namespace HomePage
{
    public class FlowerChore : PersonChore
    {
        public override string Id => "Flower";

        protected override int DaysBetween { 
            get
            {
                var today = DateTime.Now;
                var waterDayFactor = today.Month > 5 && today.Month < 9 ? 3 : 4;
                return waterDayFactor;
            }
        }

        protected override string GetLastUpdated(Settings settings) => settings.LastFlowerTime;

        protected override string GetLastUpdatedPerson(Settings settings) => settings.LastFlowerPerson;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastFlowerTime = date;
        }

        protected override void SetLastUpdatedPerson(Settings settings, string person)
        {
            settings.LastFlowerPerson = person;
        }
    }
}
