using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace HomePage.Pages
{
    public enum MovieSorting
    {
        Year,
        JensRating,
        AnnaRating,
        TotalRating,
        LastSeen
    }

    public static class MovieOrderingExtensions
    {

        public static IEnumerable<Movie> MovieSort(this IEnumerable<Movie> movies, MovieSorting sorting) =>
            sorting switch
            {
                MovieSorting.Year => movies.OrderBy(x => x.IsCompleted).ThenBy(x => x.Year),
                MovieSorting.LastSeen => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => DateHelper.FromKey(x.CompletedDate)),
                MovieSorting.JensRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.JensRanking),
                MovieSorting.AnnaRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.AnnaRanking),
                MovieSorting.TotalRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.AverageRanking),
                _ => throw new Exception(),
            };
    }

    [IgnoreAntiforgeryToken]
    public class MoviesModel : PageModel
    {
        public List<Movie> Movies { get; set; }

        public string AllMoviesJson { get; set; }

        public string SeenCounter { get; set; }

        public string LoggedInPerson { get; set; }

        public MovieSorting CurrentSorting { get; set; } = MovieSorting.Year;

        public Movie LastSeen { get; set; }

        public (MovieSorting, string)[] SortingOptions = [
            (MovieSorting.Year, "Årtal"),
            (MovieSorting.LastSeen, "Senast sedda"),
            (MovieSorting.JensRating, "Jens betyg"),
            (MovieSorting.AnnaRating, "Annas betyg"),
            (MovieSorting.TotalRating, "Totalt betyg")
        ];

        public void OnGet(string sorting)
        {
            this.TryLogIn();

            Enum.TryParse(typeof(MovieSorting), sorting, out var parsedSorting);
            if (parsedSorting is MovieSorting correctlyParsed)
            {
                CurrentSorting = correctlyParsed;
            }

            var allMovies = new MovieRepository().GetValues().Values;
            LoggedInPerson = SignInRepository.LoggedInPerson(HttpContext.Session)?.Name;
            Movies = allMovies.MovieSort(CurrentSorting).ToList();
            AllMoviesJson = JsonConvert.SerializeObject(Movies.Where(x => !x.IsCompleted));
            SeenCounter = allMovies.Count(x => x.IsCompleted) + "/" + allMovies.Count;
            LastSeen = allMovies.OrderByDescending(x => DateHelper.FromKey(x.CompletedDate)).First();
        }

        public IActionResult OnPost(string itemId, string person, int ranking)
        {
            if (this.ShouldRedirectToLogin())
            {
                return Redirect("/Login");
            }

            var repo = new MovieRepository();
            var item = repo.TryGetValue(itemId);
            if (person == null)
            {
                item.IsCompleted = true;
                item.CompletedDate = DateHelper.ToKey(DateHelper.DateNow);
            } else
            {
                if (person == Person.Jens.Name)
                {
                    item.JensRanking = ranking;
                } else
                {
                    item.AnnaRanking = ranking;
                }
            }

            repo.SaveValue(item);
            return Redirect("Movies");
        }
    }
}
