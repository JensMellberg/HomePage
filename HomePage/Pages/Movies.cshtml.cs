using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
                MovieSorting.LastSeen => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.CompletedAt),
                MovieSorting.JensRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.JensRanking),
                MovieSorting.AnnaRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.AnnaRanking),
                MovieSorting.TotalRating => movies.OrderByDescending(x => x.IsCompleted).ThenByDescending(x => x.AverageRanking),
                _ => throw new Exception(),
            };
    }

    [IgnoreAntiforgeryToken]
    public class MoviesModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public List<Movie> Movies { get; set; }

        public string AllMoviesJson { get; set; }

        public string SeenCounter { get; set; }

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
            Enum.TryParse(typeof(MovieSorting), sorting, out var parsedSorting);
            if (parsedSorting is MovieSorting correctlyParsed)
            {
                CurrentSorting = correctlyParsed;
            }

            var allMovies = dbContext.Movie.ToList();
            Movies = allMovies.MovieSort(CurrentSorting).ToList();
            AllMoviesJson = JsonConvert.SerializeObject(Movies.Where(x => !x.IsCompleted));
            SeenCounter = allMovies.Count(x => x.IsCompleted) + "/" + allMovies.Count;
            LastSeen = allMovies.OrderByDescending(x => x.CompletedAt).First();
        }

        public IActionResult OnPost(Guid itemId, string person, int ranking)
        {
            if (!IsAdmin)
            {
                return Redirect("/Login");
            }

            var item = dbContext.Movie.Find(itemId) ?? throw new Exception();
            if (person == null)
            {
                item.IsCompleted = true;
                item.CompletedAt = DateHelper.DateTimeNow;
                logger.Information($"Watched movie {item.Name}", LoggedInPerson?.UserName);
            } else
            {
                if (person == Person.Jens.Name)
                {
                    item.JensRanking = ranking;
                } else
                {
                    item.AnnaRanking = ranking;
                }

                logger.Information($"{person} gave the movie {item.Name} a score of {ranking}", LoggedInPerson?.UserName);
            }

            dbContext.SaveChanges();
            return Redirect("Movies");
        }
    }
}
