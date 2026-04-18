using HomePage.Model;

namespace HomePage.Chores
{
    public abstract class SpecificWeekDayChore(ChoreModel source) : SinglePersonChore(source)
    {
        protected override DateTime GetNextDate() => DateHelper.GetNextOfWeekday(GetLastUpdatedDate.AddDays(1), WeekDays);

        protected abstract DayOfWeek[] WeekDays { get; } 
    }
}
