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
                var validationError = Utils.ValidateImage(profilePhoto);
                if (validationError != null)
                {
                    ModelState.AddModelError(nameof(profilePhoto), validationError);
                    return Page();
                }

                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "UserPhotos");

                var fileExtension = Path.GetExtension(profilePhoto.FileName);
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
            return RedirectToPage("/Profile", new { userName = userInfo.UserName });
        }
    }
}
