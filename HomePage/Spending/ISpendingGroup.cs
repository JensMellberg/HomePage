namespace HomePage.Spending
{
    public interface ISpendingGroup
    {
        int SortOrder { get; }

        string Name { get; }

        bool IsMatch(SpendingItem item);

        bool IgnoreTowardsTotal { get; }

        string Id { get; }

        string Color { get; }
    }
}
