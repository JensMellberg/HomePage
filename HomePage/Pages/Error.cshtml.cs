using System.Diagnostics;
using HomePage.Data;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public IEnumerable<string> Errors { get; set; }

        public void OnGet()
        {
            Errors = IsAdmin
                ? dbContext.LogRow
                    .OrderByDescending(x => x.LogDate)
                    .Take(50)
                    .Select(x => x.LogDate.ToString("yy/MM/dd HH:mm:ss") + " " + x.LogRowSeverity.ToString() + " " + x.TruncatedMessage)
                    .ToList()
                : [];
        }
    }

}
