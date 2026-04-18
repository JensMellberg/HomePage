using HomePage.Model;

namespace HomePage.Chores
{
    public class FlossChore(ChoreModel source) : FlossChoreBase(source)
    {
        public const string ChoreId = "Floss";

        protected override string PersonName => Person.Anna.Name;
    }
}
