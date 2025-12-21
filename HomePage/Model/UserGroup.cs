using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class UserGroup
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public required string DisplayName { get; set; }

        public bool IsAdmin { get; set; }
    }
}
