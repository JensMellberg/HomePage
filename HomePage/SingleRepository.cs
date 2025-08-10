namespace HomePage
{
    public abstract class SingleRepository<T> : Repository<T> where T : SaveableItem, new()
    {
        public T Get()
        {
            var values = this.GetValues().Values;
            if (values.Count == 0)
            {
                return new T();
            }

            return values.First();
        }

        public void Save(T value)
        {
            this.SaveValue(value);
        }
    }
}
