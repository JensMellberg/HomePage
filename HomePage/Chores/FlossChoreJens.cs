using HomePage.Model;

namespace HomePage.Chores
{
    public class FlossChoreJens(ChoreModel source) : FlossChoreBase(source)
    {
        public const string ChoreId = "FlossJens";

        protected override string PersonName => Person.Jens.Name;
    }
}
