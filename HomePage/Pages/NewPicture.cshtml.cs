using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class NewPictureModel : PageModel
    {
        [BindProperty]
        public List<IFormFile> Uploads { get; set; }

        public IActionResult OnGet()
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await ImageRepository.Instance.UploadFiles(this.Uploads);
            return new RedirectResult("/Pictures");
        }
    }
}
