using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Spending
{
    public class SpendingItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Person { get; set; }

        public DateTime TransactionDate { get; set; }

        [MaxLength(2000)]
        public string Place { get; set; }

        public int Amount { get; set; }

        [MaxLength(200)]
        public string? SetGroupId { get; set; }

        public bool CollidesWith(SpendingItem other) => Person == other.Person && TransactionDate == other.TransactionDate && Place == other.Place && Amount == other.Amount;
    }
}
