using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class MoviesModel : PageModel
    {
        public List<Movie> Movies { get; set; }

        public string AllMoviesJson { get; set; }

        public string SeenCounter { get; set; }

        public void OnGet()
        {
            this.TryLogIn();
            var allMovies = new MovieRepository().GetValues().Values;
            Movies = allMovies.OrderBy(x => x.IsCompleted).ThenBy(x => x.Year).ToList();
            AllMoviesJson = JsonConvert.SerializeObject(Movies.Where(x => !x.IsCompleted));
            SeenCounter = allMovies.Count(x => x.IsCompleted) + "/" + allMovies.Count;
        }

        public IActionResult OnPost(string itemId)
        {
            if (this.ShouldRedirectToLogin())
            {
                return Redirect("/Login");
            }

            var repo = new MovieRepository();
            var item = repo.TryGetValue(itemId);
            item.IsCompleted = true;
            repo.SaveValue(item);

            return Redirect("Movies");
        }
    }
}
