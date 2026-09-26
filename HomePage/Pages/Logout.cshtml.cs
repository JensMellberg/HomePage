using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    public class LogoutModel(SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public IActionResult OnGet()
        {
            SignInRepository.LogOut(HttpContext.Session);
            return new RedirectResult("/Index");
        }
    }
}
