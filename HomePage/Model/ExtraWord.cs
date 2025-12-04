using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ExtraWord
    {
        [Key]
        [MaxLength(50)]
        public string Word { get; set; }

        [MaxLength(50)]
        public string Creator { get; set; }

        public bool JensApproved { get; set; }

        public bool AnnaApproved { get; set; }
    }
}
