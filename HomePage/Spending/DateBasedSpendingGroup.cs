namespace HomePage.Spending
{
    public class DateBasedSpendingGroup : ISpendingGroup 
    {
        private SpendingGroup source;
        public DateBasedSpendingGroup(SpendingGroup source)
        {
            this.source = source;
            startDate = source.StartDate ?? throw new Exception();
            endDate = source.EndDate ?? throw new Exception();
        }

        public string Name => source.Name;

        public int SortOrder => int.MinValue;

        public bool IgnoreTowardsTotal => source.IgnoreTowardsTotal;

        public string Color => source.Color;

        public bool IsMatch(SpendingItem item) => item.TransactionDate >= startDate && item.TransactionDate <= endDate;

        public Guid Id => source.Id;

        public DateTime startDate;

        public DateTime endDate;
    }
}
