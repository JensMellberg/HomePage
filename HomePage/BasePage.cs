using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage
{
    public class BasePage(SignInRepository signInRepository) : PageModel
    {
        public bool IsAdmin { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public Person? LoggedInPerson { get; private set; }

        public bool IsAdminPage { get; private set; }

        public bool RequireLogin { get; private set; }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            signInRepository.TryLogInFromCookie(HttpContext.Session, HttpContext.Request, HttpContext.Response);
            IsAdmin = SignInRepository.IsLoggedInAsAdmin(HttpContext.Session);
            IsLoggedIn = SignInRepository.IsLoggedIn(HttpContext.Session);
            LoggedInPerson = signInRepository.LoggedInPerson(HttpContext.Session);

            IsAdminPage = context.HandlerMethod?.MethodInfo.DeclaringType?
                              .GetCustomAttributes(typeof(RequireAdminAttribute), true)
                              .Any() == true;

            RequireLogin = context.HandlerMethod?.MethodInfo.DeclaringType?
                              .GetCustomAttributes(typeof(RequireLoginAttribute), true)
                              .Any() == true;

            var redirectResult = GetPotentialRedirectResult(IsAdminPage, RequireLogin);
            if (redirectResult != null)
            {
                context.Result = redirectResult;
                return;
            }

            base.OnPageHandlerExecuting(context);
        }

        public IActionResult? GetPotentialRedirectResult(bool requireAdmin, bool requireLogin)
        {
            var url = GetPotentialRedirectUrl();
            if (url == null)
            {
                return null;
            }

            return new RedirectToPageResult(url);
        }

        public JsonResult? GetPotentialClientRedirectResult(bool requireAdmin, bool requireLogin, string? customReturnUrl = null)
        {
            var url = GetPotentialRedirectUrl(requireAdmin, requireLogin, customReturnUrl);
            if (url == null)
            {
                return null;
            }

            if (url == "/AccessDenied")
            {
                return Utils.CreateAccessDeniedClientResult();
            }

            return Utils.CreateRedirectClientResult(url);
        }

        public string? GetPotentialRedirectUrl() => GetPotentialRedirectUrl(IsAdminPage, RequireLogin);

        public string? GetPotentialRedirectUrl(bool requireAdmin, bool requireLogin, string? customReturnUrl = null)
        {
            var returnUrl = customReturnUrl ?? HttpContext.Request.Path + HttpContext.Request.QueryString.Value;
            if (requireAdmin && !IsAdmin)
            {
                if (!IsLoggedIn)
                {
                    HttpContext.Session.SetString("ReturnUrl", returnUrl);
                    return "/Login";
                }
                else
                {
                    return "/AccessDenied";
                }
            }

            if (requireLogin && !IsLoggedIn)
            {
                HttpContext.Session.SetString("ReturnUrl", returnUrl);
                return "/Login";
            }

            return null;
        }
    }
}
