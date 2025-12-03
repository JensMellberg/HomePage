using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Spending
{
    public class SpendingItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [SaveProperty]
        [MaxLength(50)]
        public string Person { get; set; }

        [SaveProperty]
        [NotMapped]
        public string Date { get; set; }

        public DateTime TransactionDate { get; set; }

        [SaveProperty]
        [MaxLength(2000)]
        public string Place { get; set; }

        [SaveProperty]
        public int Amount { get; set; }

        [SaveProperty]
        [MaxLength(200)]
        public string? SetGroupId { get; set; }

        public bool CollidesWith(SpendingItem other) => Person == other.Person && Date == other.Date && Place == other.Place && Amount == other.Amount;
    }
}
