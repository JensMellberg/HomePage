using HomePage.Model;

namespace HomePage.Chores
{
    public abstract class BaseChore(ChoreModel source)
    {
        protected virtual string GetLastUpdatedPerson => source.LastUpdatedPerson ?? Person.Anna.Name;

        protected virtual void SetLastUpdatedPerson(string person) => source.LastUpdatedPerson = person;

        protected DateTime GetLastUpdatedDate => source.LastUpdated;

        protected void SetLastUpdated(DateTime date) => source.LastUpdated = date.Date;

        protected virtual string ConvertStreakPerson(string person) => person;

        public ChoreStreak GetStreak(string person)
        {
            person = ConvertStreakPerson(person);
            return source.Streaks.FirstOrDefault(x => x.Person == person) ?? throw new Exception($"No streak found for {person} on streak {source.Id}.");
        }

        protected void UpdateStreak(string person, int count)
        {
            person = ConvertStreakPerson(person);
            var streak = GetStreak(person);
            streak.Streak = count;
        }

        protected abstract int DaysBetween { get; }

        public string Id => source.Id;

        public bool TryResetStreak(List<string> exemptPersons)
        {
            if (DateHelper.AdjustedDateNow.Date > GetDateToShow())
            {
                var nextPerson = GetNextPerson();
                if (!exemptPersons.Contains(nextPerson))
                {
                    UpdateStreak(nextPerson, 0);
                    return true;
                }
            }

            return false;
        }

        public string? WasDoneToday()
        {
            if (source.LastUpdated == DateHelper.DateNow)
            {
                return GetLastUpdatedPerson;
            }

            return null;
        }

        public string? ChorePerson()
        {
            if (DateHelper.AdjustedDateNow >= GetDateToShow())
            {
                return GetNextPerson();
            }

            return null;
        }

        private int IncreaseStreak(string person)
        {
            var current = GetStreak(person).Streak;
            UpdateStreak(person, current + 1);
            return current + 1;
        }

        protected virtual string GetNextPerson() => GetLastUpdatedPerson == Person.Jens.Name ? Person.Anna.Name : Person.Jens.Name;

        private DateTime GetDateToShow()
        {
            var lastUpdated = GetLastUpdatedDate;
            return lastUpdated.AddDays(DaysBetween);
        }

        public int Update()
        {
            var lastUpdatedPerson = GetLastUpdatedPerson;
            var streak = 0;
            if (lastUpdatedPerson == Person.Jens.Name)
            {
                SetLastUpdatedPerson(Person.Anna.Name);
                streak = IncreaseStreak(Person.Anna.Name);
            }
            else
            {
                SetLastUpdatedPerson(Person.Jens.Name);
                streak = IncreaseStreak(Person.Jens.Name);
            }

            SetLastUpdated(DateHelper.DateNow);
            return streak;
        }
    }
}
