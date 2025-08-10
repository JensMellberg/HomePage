namespace HomePage
{
    public class ToDoItem : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public bool IsCompleted { get; set; }
    }
}
