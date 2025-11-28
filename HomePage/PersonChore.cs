namespace HomePage
{
    public abstract class PersonChore
    {
        protected abstract string GetLastUpdated(Settings settings);

        protected abstract void SetLastUpdated(Settings settings, string date);

        protected abstract string GetLastUpdatedPerson(Settings settings);

        protected abstract void SetLastUpdatedPerson(Settings settings, string person);

        protected virtual string ConvertStreakPerson(string person) => person;

        public int GetStreak(string person) => GetStreak(new SettingsRepository().Get(), person);

        protected int GetStreak(Settings settings, string person)
        {
            person = ConvertStreakPerson(person);
            var allStreaks = settings.Streaks;
            var personStreaks = allStreaks.Where(x => x.StartsWith(person)).Select(x => x.Substring(person.Length));
            var streak = personStreaks.FirstOrDefault(x => x.StartsWith(Id))?.Substring(Id.Length);
            return int.TryParse(streak, out var intStreak) ? intStreak : 0;
        }

        protected void UpdateStreak(Settings settings, string person, int count)
        {
            person = ConvertStreakPerson(person);
            var allStreaks = settings.Streaks;
            var updatedStreak = person + Id + count;
            var flag = false;
            for (var i = 0; i < settings.Streaks.Count; i++)
            {
                if (allStreaks[i].StartsWith(person + Id))
                {
                    allStreaks[i] = updatedStreak;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                allStreaks.Add(updatedStreak);
            }

            settings.Streaks = allStreaks;
            new SettingsRepository().Save(settings);
        }

        protected abstract int DaysBetween { get; }

        public abstract string Id { get; }

        public void TryResetStreak(List<string> exemptPersons)
        {
            var settings = new SettingsRepository().Get();
            if (DateHelper.AdjustedDateNow.Date > GetDateToShow(settings))
            {
                var nextPerson = GetNextPerson(settings);
                if (!exemptPersons.Contains(nextPerson))
                {
                    UpdateStreak(settings, GetNextPerson(settings), 0);
                }
            }
        }

        public string WasDoneToday()
        {
            var settings = new SettingsRepository().Get();
            var lastUpdated = GetLastUpdated(settings);
            if (lastUpdated == DateHelper.ToKey(DateHelper.DateNow))
            {
                return GetLastUpdatedPerson(settings);
            }

            return null;
        }

        public string ChorePerson()
        {
            var settings = new SettingsRepository().Get();
            if (DateHelper.AdjustedDateNow >= GetDateToShow(settings))
            {
                return GetNextPerson(settings);
            }

            return null;
        }

        private int IncreaseStreak(Settings settings, string person)
        {
            var current = GetStreak(settings, person);
            UpdateStreak(settings, person, current + 1);
            return current + 1;
        }

        protected virtual string GetNextPerson(Settings settings) => (GetLastUpdatedPerson(settings) ?? Person.Anna.Name) == Person.Jens.Name ? Person.Anna.Name : Person.Jens.Name;

        private DateTime GetDateToShow(Settings settings)
        {
            var lastUpdated = GetLastUpdated(settings);
            var lastUpdatedDate = string.IsNullOrEmpty(lastUpdated) ? DateTime.MinValue : DateHelper.FromKey(lastUpdated);
            return lastUpdatedDate.AddDays(DaysBetween);
        }

        public int Update()
        {
            var repo = new SettingsRepository();
            var settings = repo.Get();
            var lastUpdatedPerson = GetLastUpdatedPerson(settings);
            var streak = 0;
            if (lastUpdatedPerson == Person.Jens.Name)
            {
                SetLastUpdatedPerson(settings, Person.Anna.Name);
                streak = IncreaseStreak(settings, Person.Anna.Name);
            }
            else
            {
                SetLastUpdatedPerson(settings, Person.Jens.Name);
                streak = IncreaseStreak(settings, Person.Jens.Name);
            }

            SetLastUpdated(settings, DateHelper.ToKey(DateHelper.AdjustedDateNow));
            repo.Save(settings);
            return streak;
        }
    }
}
