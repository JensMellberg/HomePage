using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [ApiController]
    [Route("ExternalLogin")]
    public class ExternalLoginController(SignInRepository signInRepository) : ControllerBase
    {
        [HttpPost("Login")]
        public IActionResult ExternalLogin([FromBody] LoginDetails loginDetails)
        {
            var user = signInRepository.VerifyUserCredentials(Request, loginDetails.username, loginDetails.password);
            if (user != null)
            {
                var cookie = signInRepository.AddCookieForUser(user);
                return new JsonResult(new { success = true, accessToken = cookie });
            }

            return new JsonResult(new { success = false });
        }

        [HttpPost("VerifyToken")]
        public IActionResult VerifyToken([FromBody] ExternalAccessDetails accessDetails)
        {
            return new JsonResult(new { success = signInRepository.VerifyAuthCookie(accessDetails.username, accessDetails.accessToken) });
        }

        public class LoginDetails
        {
            public string username { get; set; }
            public string password { get; set; }
        }
    }

    public class ExternalAccessDetails
    {
        public string username { get; set; }
        public Guid accessToken { get; set; }
    }
}
