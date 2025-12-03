namespace HomePage
{
    public class WorkoutChore : SinglePersonChore
    {
        public override string Id => "Workout";

        protected override int DaysBetween => 4;

        protected override string PersonName => Person.Anna.Name;

        protected override string GetLastUpdated(SettingsTemp settings) => settings.LastWorkoutTime;

        protected override void SetLastUpdated(SettingsTemp settings, string date)
        {
            settings.LastWorkoutTime = date;
        }
    }
}
