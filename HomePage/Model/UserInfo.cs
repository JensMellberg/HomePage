using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class UserInfo
    {
        [Key]
        [MaxLength(50)]
        public required string UserName { get; set; }

        [MaxLength(50)]
        public required string DisplayName { get; set; }

        public required byte[] PasswordHash { get; set; }

        public List<SignInCookie> Cookies { get; set; } = [];

        public bool IsAdmin { get; set; }
    }
}
