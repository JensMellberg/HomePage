using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class PicturesModel(SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public IEnumerable<string> AllPictures { get; set; }

        public string TotalFileSize { get; set; }

        public void OnGet()
        {
            var pair = ImageRepository.Instance.GetAll();
            AllPictures = pair.Item1.Select(x => x.Replace("Pictures\\", "").Replace("Pictures/", ""));
            TotalFileSize = Math.Round(((double)pair.Item2) / 1000000, 2).ToString() + " MB";
        }

        public IActionResult OnPost(string url)
        {
            ImageRepository.Instance.DeleteImage(url);
            return new JsonResult(new { success = true });
        }
    }
}
