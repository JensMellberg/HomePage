using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateMovieModel : PageModel
    {
        public Movie Movie { get; set; }
        public IActionResult OnGet(string id)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                Movie = new Movie();
            }
            else
            {
                Movie = new MovieRepository().TryGetValue(id) ?? new Movie();
            }

            return Page();
        }

        public IActionResult OnPost(string id, string name, int year, string imageUrl)
        {
            var movie = new Movie { Key = id, Name = name, Year = year, ImageUrl = imageUrl };
            new MovieRepository().SaveValue(movie);

            return Redirect($"/Movies");
        }
    }
}
