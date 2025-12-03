namespace HomePage.Spending
{
    public interface ISpendingGroup
    {
        int SortOrder { get; }

        string Name { get; }

        bool IsMatch(SpendingItem item);

        bool IgnoreTowardsTotal { get; }

        Guid Id { get; }

        string Color { get; }
    }
}
