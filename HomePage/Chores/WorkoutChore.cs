using HomePage.Model;

namespace HomePage.Chores
{
    public class WorkoutChore(ChoreModel source) : SinglePersonChore(source)
    {
        public const string ChoreId = "Workout";

        protected override string PersonName => Person.Anna.Name;

        protected override int DaysBetween => 4;
    }
}
