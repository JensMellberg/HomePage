namespace HomePage
{
    public class ThemeDay : SaveableItem
    {
        public string Key { get; set; }

        [SaveProperty]
        public string Date { get; set; }

        [SaveProperty]
        public string DayName { get; set; }
    }
}
