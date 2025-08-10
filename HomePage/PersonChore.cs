namespace HomePage
{
    public abstract class PersonChore
    {
        protected abstract string GetLastUpdated(Settings settings);

        protected abstract void SetLastUpdated(Settings settings, string date);

        protected abstract string GetLastUpdatedPerson(Settings settings);

        protected abstract void SetLastUpdatedPerson(Settings settings, string person);

        protected abstract int DaysBetween { get; }

        public abstract string Id { get; }

        public string WasDoneToday()
        {
            var settings = new SettingsRepository().Get();
            var lastUpdated = GetLastUpdated(settings);
            if (lastUpdated == DateHelper.ToKey(DateTime.Now))
            {
                return GetLastUpdatedPerson(settings);
            }

            return null;
        }

        public string ChorePerson()
        {
            var settings = new SettingsRepository().Get();
            var lastUpdated = GetLastUpdated(settings);
            var lastUpdatedDate = string.IsNullOrEmpty(lastUpdated) ? DateTime.MinValue : DateHelper.FromKey(lastUpdated);
            if (DateTime.Now >= lastUpdatedDate.AddDays(DaysBetween))
            {
                return GetNextPerson(settings);
            }

            return null;
        }

        protected virtual string GetNextPerson(Settings settings) => (GetLastUpdatedPerson(settings) ?? Person.Anna.Name) == Person.Jens.Name ? Person.Anna.Name : Person.Jens.Name;

        public void Update()
        {
            var repo = new SettingsRepository();
            var settings = repo.Get();
            SetLastUpdated(settings, DateHelper.ToKey(DateTime.Now));
            var lastUpdatedPerson = GetLastUpdatedPerson(settings);
            if (lastUpdatedPerson == Person.Jens.Name)
            {
                SetLastUpdatedPerson(settings, Person.Anna.Name);
            }
            else
            {
                SetLastUpdatedPerson(settings, Person.Jens.Name);
            }

            repo.Save(settings);
        }
    }
}
