namespace HomePage
{
    public class ToDoRepository : Repository<ToDoItem>
    {
        public override string FileName => "ToDo.txt";
    }
}
