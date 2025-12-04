using HomePage.Model;

namespace HomePage.Chores
{
    public class EyeChore(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "Eye";

        protected override string PersonName => Person.Jens.Name;

        protected override int DaysBetween => 31;
    }
}
