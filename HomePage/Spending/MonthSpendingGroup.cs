using HomePage.Spending;

namespace HomePage
{
    public class MonthSpendingGroup(ISpendingGroup source)
    {
        public List<SpendingItem> Entries { get; private set; } = [];

        public bool IgnoreTowardsTotal { get; private set; } = source.IgnoreTowardsTotal;

        public string Name { get; private set; } = source.Name;

        public int SortOrder { get; private set; } = source.SortOrder;

        public int Total => Entries.Sum(x => x.Amount);

        public int TotalWithIgnore => IgnoreTowardsTotal ? 0 : Total;

        public string Color { get; private set; } = source.Color;

        public string Id { get; private set; } = source.Id;
    }
}
