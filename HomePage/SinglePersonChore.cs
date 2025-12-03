namespace HomePage
{
    public abstract class SinglePersonChore : PersonChore
    {
        protected abstract string PersonName { get; }

        protected override string GetLastUpdatedPerson(SettingsTemp settings) => PersonName;

        protected override void SetLastUpdatedPerson(SettingsTemp settings, string person) { }

        protected override string GetNextPerson(SettingsTemp settings) => PersonName;

        protected override string ConvertStreakPerson(string person) => PersonName;
    }
}
