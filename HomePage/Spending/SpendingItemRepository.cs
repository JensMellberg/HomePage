namespace HomePage.Spending
{
    public class SpendingItemRepository : Repository<SpendingItem>
    {
        public override string FileName => "Spending.txt";
    }
}
