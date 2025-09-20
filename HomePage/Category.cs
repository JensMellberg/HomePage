namespace HomePage
{
    public class Category : SaveableItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public int GoalPerWeek { get; set; }

        [SaveProperty]
        public bool IsBad { get; set; }

        [SaveProperty]
        public bool NeedsOnAllSides { get; set; }

        public bool HasGoal => GoalPerWeek > 0;

        public string GoalPerWeekText => HasGoal && !IsBad ? GoalPerWeek.ToString() : "";

        public string BadGoalPerWeekText => HasGoal && IsBad ? GoalPerWeek.ToString() : "";
    }
}
