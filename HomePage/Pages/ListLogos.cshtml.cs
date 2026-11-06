using HomePage.Data;
using HomePage.LogoGame;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [RequireLogin]
    public class ListLogosModel(LogotypeRepository logotypeRepository, AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public required IEnumerable<LogotypeMetadata> AllLogos { get; set; }

        public IActionResult OnGet()
        {
            AllLogos = logotypeRepository.LoadLogos(LoggedInPerson!.UserName);
            return Page();
        }

        public IActionResult OnPost(string logoId)
        {
            var logo = logotypeRepository.LoadLogoFromId(logoId);
            if (LoggedInPerson!.UserName != logo.GetCreator && !IsAdmin)
            {
                logger.Warning($"{LoggedInPerson!.UserName} tried to delete a logo that was not theirs. {logo.Id}");
                return BadRequest();
            }

            if (dbContext.DayLogo.Any(x => x.LogoMetadataPath == logoId))
            {
                return Utils.CreateErrorClientResult("Kan inte ta bort en logga som har spelats.");
            }

            logotypeRepository.DeleteLogo(logoId);
            logger.Information($"{LoggedInPerson!.UserName} deleted a logo with answer {logo.Answers[0]}");
            return Utils.CreateClientResult(new
            {
                success = true
            });
        }
    }
}
