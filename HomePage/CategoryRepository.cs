namespace HomePage
{
    public class CategoryRepository : Repository<Category>
    {
        public override string FileName => "Category.txt";
    }
}
