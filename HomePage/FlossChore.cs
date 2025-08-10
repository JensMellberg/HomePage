namespace HomePage
{
    public class FlossChore : SinglePersonChore
    {
        public override string Id => "Floss";

        protected override int DaysBetween => 7;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(Settings settings)
        {
            var actualDate = DateHelper.FromKey(settings.LastFlossTime);
            if (actualDate == DateTime.Now.Date)
            {
                return settings.LastFlossTime;
            } 
            else
            {
                return DateHelper.ToKey(DateHelper.GetFirstOfWeek(actualDate));
            }
        }

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastFlossTime = date;
        }
    }
}
