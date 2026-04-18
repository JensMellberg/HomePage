using HomePage.Model;

namespace HomePage.Chores
{
    public abstract class FlossChoreBase(ChoreModel source) : SpecificWeekDayChore(source)
    {
        protected override DayOfWeek[] WeekDays => [DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Sunday];
    }
}
