using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class NewAccountModel(SignInRepository signInRepository) : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost(string username, string password, string name)
        {
            var success = username != Person.MrRobot.UserName && signInRepository.CreateAccount(username, password, name);
            return new JsonResult(new { success });
        }
    }
}
