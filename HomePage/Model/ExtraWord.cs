using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ExtraWord
    {
        [Key]
        [MaxLength(50)]
        public string Word { get; set; }

        [SaveProperty]
        [MaxLength(50)]
        public string Creator { get; set; }

        [SaveProperty]
        public bool JensApproved { get; set; }

        [SaveProperty]
        public bool AnnaApproved { get; set; }
    }
}
