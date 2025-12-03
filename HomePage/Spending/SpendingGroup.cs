using System.ComponentModel.DataAnnotations;

namespace HomePage.Spending
{

    public class SpendingGroup
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public List<string> Patterns { get; set; } = [];

        public bool IgnoreTowardsTotal { get; set; }

        [MaxLength(50)]
        public string Person { get; set; }

        [MaxLength(50)]
        public string Color { get; set; }

        [SaveProperty]
        public DateTime? StartDate { get; set; }

        [SaveProperty]
        public DateTime? EndDate { get; set; }

        public bool IsDateBasedGroup => StartDate.HasValue;
    }
}
