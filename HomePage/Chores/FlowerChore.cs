using HomePage.Model;

namespace HomePage.Chores
{
    public class FlowerChore(ChoreModel source) : BaseChore(source)
    {
        public const string ChoreId = "Flower";

        protected override int DaysBetween
        {
            get
            {
                var today = DateTime.Now;
                var waterDayFactor = today.Month > 5 && today.Month < 9 ? 3 : 4;
                return waterDayFactor;
            }
        }
    }
}
