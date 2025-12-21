using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireLogin]
    public class CreateMovieModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public Movie Movie { get; set; }
        public IActionResult OnGet(Guid id)
        {
            if (id == Guid.Empty)
            {
                Movie = new Movie();
            }
            else
            {
                Movie = dbContext.Movie.Find(id) ?? new Movie();
                if (!IsAdmin && Movie.Owner != LoggedInPerson?.UserName)
                {
                    return new RedirectToPageResult("/AccessDenied");
                }
            }

            return Page();
        }

        public IActionResult OnPost(Guid id, string name, int year, string imageUrl)
        {
            var existing = dbContext.Movie.Find(id);
            if (existing != null)
            {
                if (!IsAdmin && existing.Owner != LoggedInPerson?.UserName)
                {
                    logger.Warning($"{LoggedInPerson?.UserName} tried to update movie {existing.Name} but did not have permission.", LoggedInPerson?.UserName);
                    return Unauthorized();
                }

                logger.Information($"{LoggedInPerson?.UserName} updated the movie {existing.Name} to {name}.", LoggedInPerson?.UserName);
                existing.Name = name;
                existing.Year = year;
                existing.ImageUrl = imageUrl;
            } else
            {
                var movie = new Movie { Id = id, Name = name, Year = year, ImageUrl = imageUrl, Owner = LoggedInPerson?.UserName ?? "" };
                dbContext.Movie.Add(movie);
                logger.Information($"{LoggedInPerson?.UserName} added the movie {name}.", LoggedInPerson?.UserName);
            }

            dbContext.SaveChanges();
            return Redirect($"/Movies");
        }
    }
}
