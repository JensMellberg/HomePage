namespace HomePage
{
    public class PersonSpecific<T>
    {
        private Dictionary<string, T> Values { get; set; }

        public T Get(Person person) => Get(person.Name);

        public T Get(string person)
        {
            if (Values.TryGetValue(person, out var value))
            {
                return value;
            }

            return default;
        }

        public void Set(string person, T value)
        {
            Values[person] = value;
        }

        public void Set(Person person, T value) => Set(person.Name, value);

        public string ToString()
        {
            return string.Join(",", Values.Where(x => x.Value != null).Select(x => x.Key + ":" + x.Value.ToString()));
        }
    }
}
