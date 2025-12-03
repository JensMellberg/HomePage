using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class DownloadBackupModel(SignInRepository signInRepository, SettingsRepository settingsRepository) : BasePage(signInRepository)
    {
        public ActionResult OnGet()
        {
            settingsRepository.PerformBackup(true);

            var backupStream = settingsRepository.GetLatestBackup(out var fileName);
            if (backupStream == null)
            {
                return NotFound();
            }

            return File(backupStream, "application/octet-stream", fileName);
        }
    }
}
