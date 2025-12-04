using HomePage.Model;

namespace HomePage.Chores
{
    public class FlossChoreJens(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "FlossJens";

        protected override string PersonName => Person.Jens.Name;

        protected override int DaysBetween => 3;
    }
}
