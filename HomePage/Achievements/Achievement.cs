namespace HomePage.Achievements
{
    public abstract class Achievement
    {
        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract int Points { get; }

        public abstract string Icon { get; }

        public abstract int Id { get; }

        public abstract bool AdminOnly { get; }
    }
}
