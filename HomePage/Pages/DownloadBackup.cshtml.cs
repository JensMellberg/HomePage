using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class DownloadBackupModel : PageModel
    {
        public ActionResult OnGet()
        {
            var fileName = new SettingsRepository().PerformBackup(true);
            var fullPath = "";
            while (!System.IO.File.Exists(fileName))
            {
                Thread.Sleep(100);
            }

            fullPath = Path.GetFullPath(fileName);
            return PhysicalFile(fullPath, "application/zip", fileName.Replace("Backups/", ""));
        }
    }
}
