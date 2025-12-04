using HomePage.Model;

namespace HomePage.Chores
{
    public abstract class SinglePersonChore(ChoreModel source) : BaseChore(source)
    {
        protected abstract string PersonName { get; }

        protected override string GetLastUpdatedPerson => PersonName;

        protected override void SetLastUpdatedPerson(string person) { }

        protected override string GetNextPerson() => PersonName;

        protected override string ConvertStreakPerson(string person) => PersonName;
    }
}
