using HomePage.Model;

namespace HomePage.Chores
{
    public class RecycleChore(ChoreModel source) : BaseChore(source)
    {
        public const string ChoreId = "Recycle";

        protected override DateTime GetNextDate() => GetLastUpdatedDate.AddDays(5);
    }
}
