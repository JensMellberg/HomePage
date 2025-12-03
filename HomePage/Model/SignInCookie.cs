using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class SignInCookie
    {
        [Key]
        public Guid CookieId { get; set; }

        public string UserId { get; set; }

        public UserInfo User { get; set; }

        public DateTime Expires { get; set; }
    }
}
