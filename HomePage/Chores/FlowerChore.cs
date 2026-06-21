using HomePage.Model;

namespace HomePage.Chores
{
    public class FlowerChore(ChoreModel source) : BaseChore(source)
    {
        public const string ChoreId = "Flower";

        protected override DateTime GetNextDate()
        {
            var today = DateHelper.DateNow;
            int daysBetween;
            if (today.Month < 5 || today.Month > 8)
            {
                daysBetween = 4;
            }
            else if (today.Month == 5 || today.Month == 8)
            {
                daysBetween = 3;
            }
            else
            {
                daysBetween = 2;
            }
            return GetLastUpdatedDate.AddDays(daysBetween);
        }
    }
}
