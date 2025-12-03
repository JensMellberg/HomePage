using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    public class LogoutModel(SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public IActionResult OnGet()
        {
            signInRepository.LogOut(HttpContext.Session);
            return new RedirectResult("/Index");
        }
    }
}
