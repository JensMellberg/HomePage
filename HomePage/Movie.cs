namespace HomePage
{
    public class Movie : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public int Year { get; set; } = 2000;

        [SaveProperty]
        public string ImageUrl { get; set; }

        [SaveProperty]
        public bool IsCompleted { get; set; }
    }
}
