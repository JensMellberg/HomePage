namespace HomePage
{
    public abstract class SinglePersonChore : PersonChore
    {
        protected abstract string PersonName { get; }

        protected override string GetLastUpdatedPerson(Settings settings) => PersonName;

        protected override void SetLastUpdatedPerson(Settings settings, string person) { }

        protected override string GetNextPerson(Settings settings) => PersonName;
    }
}
