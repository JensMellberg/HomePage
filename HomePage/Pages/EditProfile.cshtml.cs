using HomePage.Data;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireLogin]
    public class EditProfileModel(SignInRepository signInRepository, AppDbContext dbContext) : BasePage(signInRepository)
    {
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost(string displayName, IFormFile? profilePhoto)
        {
            var userInfo = SignInRepository.GetUserInfo(LoggedInPerson!.UserName)!;
            if (!Utils.IsAcceptableName(displayName))
            {
                ModelState.AddModelError(nameof(profilePhoto), "Ogiltigt namn.");
                return Page();
            }

            userInfo.DisplayName = displayName;

            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                const long maxFileSize = 10 * 1024 * 1024;
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp",
                    ".bmp"
                };

                if (profilePhoto.Length > maxFileSize)
                {
                    ModelState.AddModelError(nameof(profilePhoto), "Bilden får vara högst 10 MB.");
                    return Page();
                }

                if (!profilePhoto.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError(nameof(profilePhoto), "Bilden har inte en godkänd content type.");
                    return Page();
                }

                var fileExtension = Path.GetExtension(profilePhoto.FileName);
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(nameof(profilePhoto), "Bilden har inte en godkänd filtyp.");
                    return Page();
                }

                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "UserPhotos");

                var fileName = Guid.NewGuid() + fileExtension;
                var filePath = Path.Combine(folder, fileName);

                if (!string.IsNullOrEmpty(userInfo.PhotoUrl)) {
                    System.IO.File.Delete(Path.Combine(folder, userInfo.PhotoUrl));
                }

                using var stream = System.IO.File.Create(filePath);
                profilePhoto.CopyTo(stream);
                userInfo.PhotoUrl = fileName;
            }

            dbContext.SaveChanges();
            return Redirect($"/Profile?userName={userInfo.UserName}");
        }
    }
}
