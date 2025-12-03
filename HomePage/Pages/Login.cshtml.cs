using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel(SignInRepository signInRepository) : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string username, string password)
        {
            var success = signInRepository.LogInWithPassword(HttpContext.Session, Response, username, password);
            if (success)
            {
                return new JsonResult(new { success = true, url = HttpContext.Session.GetString("ReturnUrl") });
            }

            return new JsonResult(new { success = false });
        }
    }
}
