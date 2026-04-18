using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using PokemonTDEngine;

namespace HomePage.Pages
{
    public class PokemonTDLeaderboardModel(AppDbContext dbContext, SignInRepository signInRepository, PokemonTDResultRepository pokemonTDResultRepository) : BasePage(signInRepository)
    {
        public Dictionary<string, string> NamesByUsername { get; set; }
        public Dictionary<Difficulty, List<PokemonTDResult>> ResultsPerDifficulty { get; set; } 
        public IActionResult OnGet()
        {
            NamesByUsername = dbContext.UserInfo.ToDictionary(x => x.UserName.ToLower(), x => x.DisplayName);
            ResultsPerDifficulty = dbContext.PokemonTDResult
                .GroupBy(x => x.Difficulty)
                .ToDictionary(
                x => x.Key, 
                x => x.OrderByDescending(x => x.DamageTestResult).ThenByDescending(x => x.LevelCompleted).ToList());
            return Page();
        }
    }
}
