using HomePage.Model;

namespace HomePage.Chores
{
    public class FlowerChore(ChoreModel source) : BaseChore(source)
    {
        public const string ChoreId = "Flower";

        protected override DateTime GetNextDate()
        {
            var today = DateHelper.DateNow;
            var daysBetween = today.Month > 5 && today.Month < 9 ? 3 : 4;
            return GetLastUpdatedDate.AddDays(daysBetween);
        }
    }
}
