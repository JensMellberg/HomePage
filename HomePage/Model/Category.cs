using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class Category
    {
        [Key]
        [MaxLength(50)]
        public string Key { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(50)]
        public string Name { get; set; }

        public int GoalPerWeek { get; set; }

        public bool IsBad { get; set; }

        public bool NeedsOnAllSides { get; set; }

        public List<Food> Food { get; set; }

        public bool HasGoal => GoalPerWeek > 0;

        public string GoalPerWeekText => HasGoal && !IsBad ? GoalPerWeek.ToString() : "";

        public string BadGoalPerWeekText => HasGoal && IsBad ? GoalPerWeek.ToString() : "";
    }
}
