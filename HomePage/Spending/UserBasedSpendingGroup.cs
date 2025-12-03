namespace HomePage.Spending
{
    public class UserBasedSpendingGroup : ISpendingGroup 
    {
        private SpendingGroup source;
        public UserBasedSpendingGroup(SpendingGroup source)
        {
            this.source = source;
            patterns = source.Patterns.Select(SpendingGroupPattern.FromString).ToList();
        }

        public string Name => source.Name;

        public int SortOrder => source.SortOrder;

        public bool IgnoreTowardsTotal => source.IgnoreTowardsTotal;

        public string Color => source.Color;

        public bool IsMatch(SpendingItem item) => patterns.Any(x => x.Matches(item.Place));

        private List<SpendingGroupPattern> patterns;

        public Guid Id => source.Id;
    }
}
