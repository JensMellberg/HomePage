namespace HomePage.Spending
{
    public class SpendingGroup : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public int SortOrder { get; set; }

        [SaveProperty]
        [SaveAsList]
        public List<string> Patterns { get; set; } = [];

        [SaveProperty]
        public bool IgnoreTowardsTotal { get; set; }

        [SaveProperty]
        public string Person { get; set; }

        [SaveProperty]
        public string Color { get; set; }

        [SaveProperty]
        public string StartDate { get; set; }

        [SaveProperty]
        public string EndDate { get; set; }

        public bool IsDateBasedGroup => !string.IsNullOrEmpty(StartDate);
    }
}
