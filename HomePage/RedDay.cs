namespace HomePage
{
    public class RedDay : SaveableItem
    {
        public string Key { get; set; }

        [SaveProperty]
        public bool IsRed { get; set; }

        [SaveProperty]
        public string DayName { get; set; }
    }
}
