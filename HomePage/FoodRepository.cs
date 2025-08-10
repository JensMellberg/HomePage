namespace HomePage
{
    public class FoodRepository : Repository<Food>
    {
        public override string FileName => "Food.txt";
    }
}
