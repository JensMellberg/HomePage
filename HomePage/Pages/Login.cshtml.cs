using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string password)
        {
            var success = SignInRepository.LogInWithPassword(HttpContext.Session, Response, password);
            if (success)
            {
                return new JsonResult(new { success = true, url = HttpContext.Session.GetString("ReturnUrl")});
            }

            return new JsonResult(new { success = false });
        }
    }
}
