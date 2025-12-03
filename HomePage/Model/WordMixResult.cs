using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomePage.Model
{
    public class WordMixResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Date { get; set; }

        [SaveProperty]
        [MaxLength(50)]
        public string Person { get; set; }

        [SaveProperty]
        [MaxLength(200)]
        public string Board { get; set; }

        [SaveProperty]
        public int Score { get; set; }
    }
}
