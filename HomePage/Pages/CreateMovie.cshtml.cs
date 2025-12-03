using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CreateMovieModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
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
            }

            return Page();
        }

        public IActionResult OnPost(Guid id, string name, int year, string imageUrl)
        {
            var existing = dbContext.Movie.Find(id);
            if (existing != null)
            {
                existing.Name = name;
                existing.Year = year;
                existing.ImageUrl = imageUrl;
            } else
            {
                var movie = new Movie { Id = id, Name = name, Year = year, ImageUrl = imageUrl };
                dbContext.Movie.Add(movie);
            }

            dbContext.SaveChanges();
            return Redirect($"/Movies");
        }
    }
}
