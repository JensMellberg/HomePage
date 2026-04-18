using HomePage.Data;
using HomePage.Model;
using PokemonTDEngine;

namespace HomePage.Repositories
{
    public class PokemonTDResultRepository(AppDbContext dbContext, DatabaseLogger logger)
    {
        public Dictionary<Difficulty, PokemonTDResult> GetResults(string person)
        {
            var results = new Dictionary<Difficulty, PokemonTDResult>();
            foreach (var existing in dbContext.PokemonTDResult.Where(x => x.Person == person))
            {
                results[existing.Difficulty] = existing;
            }

            return results;
        }

        public bool AddResult(string person, Difficulty difficulty, List<Event> playerEvents)
        {
            var existing = dbContext.PokemonTDResult.FirstOrDefault(x => x.Person == person && x.Difficulty == difficulty);
            var simulator = new GameSimulator(playerEvents, difficulty);
            try
            {
                simulator.SimulateGame();
            }
            catch (Exception e)
            {
                logger.Log(LogRowSeverity.Error, $"Error when saving PokemonTD result {e.Message}", person, e.StackTrace);
                return false;
            }

            var result = simulator.Result;
            if (result == null)
            {
                logger.Error($"Were not able to simulate PokemonTD game result", person);
                return false;
            }

            var existingGameResult = existing?.ToGameResult;
            if (existingGameResult != null && !IsBetterThan(result, existingGameResult))
            {
                logger.Warning($"Submitted a PokemonTD result worse than the existing.", person);
                return false;
            }

            if (existing != null)
            {
                existing.IsWin = result.IsWin;
                existing.DamageTestResult = result.DamageTestResult;
                existing.LevelCompleted = result.LevelCompleted;
            }
            else
            {
                dbContext.PokemonTDResult.Add(PokemonTDResult.FromGameResult(result, person));
            }

            dbContext.SaveChanges();
            return true;

            bool IsBetterThan(GameResult score, GameResult existing) => score.DamageTestResult > existing.DamageTestResult || score.LevelCompleted > existing.LevelCompleted;
        }
    }
}
