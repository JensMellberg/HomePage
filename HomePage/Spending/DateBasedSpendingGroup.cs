namespace HomePage.Spending
{
    public class DateBasedSpendingGroup : ISpendingGroup 
    {
        private SpendingGroup source;
        public DateBasedSpendingGroup(SpendingGroup source)
        {
            this.source = source;
            this.startDate = DateHelper.FromKey(source.StartDate);
            this.endDate = DateHelper.FromKey(source.EndDate);
        }

        public string Name => source.Name;

        public int SortOrder => int.MinValue;

        public bool IgnoreTowardsTotal => source.IgnoreTowardsTotal;

        public string Color => source.Color;

        public bool IsMatch(SpendingItem item) => item.ConvertedDate >= this.startDate && item.ConvertedDate <= this.endDate;

        public string Id => source.Key;

        public DateTime startDate;

        public DateTime endDate;
    }
}
