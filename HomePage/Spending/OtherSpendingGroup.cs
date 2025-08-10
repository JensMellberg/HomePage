namespace HomePage.Spending
{
    public class OtherSpendingGroup : ISpendingGroup
    {
        public int SortOrder => int.MaxValue;

        public string Name => "Annat";

        public bool IgnoreTowardsTotal => false;

        public bool IsMatch(SpendingItem item) => true;

        public string Id => null;

        public string Color => "#d67dd7";
    }
}
