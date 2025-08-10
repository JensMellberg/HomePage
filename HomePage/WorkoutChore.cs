namespace HomePage
{
    public class WorkoutChore : SinglePersonChore
    {
        public override string Id => "Workout";

        protected override int DaysBetween => 4;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(Settings settings) => settings.LastWorkoutTime;

        protected override void SetLastUpdated(Settings settings, string date)
        {
            settings.LastWorkoutTime = date;
        }
    }
}
