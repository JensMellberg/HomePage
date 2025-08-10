using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class PicturesModel : PageModel
    {
        public IEnumerable<string> AllPictures { get; set; }

        public string TotalFileSize { get; set; }

        public bool IsLoggedIn { get; set; }

        public void OnGet()
        {
            this.TryLogIn();
            IsLoggedIn = SignInRepository.IsLoggedIn(HttpContext.Session);
            var pair = ImageRepository.Instance.GetAll();
            AllPictures = pair.Item1.Select(x => x.Replace("Pictures\\", "").Replace("Pictures/", ""));
            TotalFileSize = Math.Round(((double)pair.Item2) / 1000000, 2).ToString() + " MB";
        }

        public IActionResult OnPost(string url)
        {
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            ImageRepository.Instance.DeleteImage(url);
            return new JsonResult(new { success = true });
        }
    }
}
