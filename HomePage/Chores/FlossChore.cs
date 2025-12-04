using HomePage.Model;

namespace HomePage.Chores
{
    public class FlossChore(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "Floss";

        protected override string PersonName => Person.Anna.Name;

        protected override int DaysBetween => 3;
    }
}
