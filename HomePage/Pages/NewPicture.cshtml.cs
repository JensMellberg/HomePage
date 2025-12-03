using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class NewPictureModel(SignInRepository signInRepository) : BasePage(signInRepository)
    {
        [BindProperty]
        public List<IFormFile> Uploads { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await ImageRepository.Instance.UploadFiles(this.Uploads);
            return new RedirectResult("/Pictures");
        }
    }
}
