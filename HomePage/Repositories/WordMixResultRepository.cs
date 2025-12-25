using HomePage.Data;
using HomePage.Model;

namespace HomePage.Repositories
{
    public class WordMixResultRepository(AppDbContext dbContext, CurrentWordMixRepository currentWordMixRepository, DatabaseLogger logger)
    {
        public List<IGrouping<DateTime, WordMixResult>> GetResultsByDay()
        {
            return dbContext.WordMixResult
                .ToList()
                .GroupBy(x => x.Date)
                .OrderByDescending(x => x.Key)
                .ToList();
        }

        public void UpdateResultWithValidation(string person, string letters, int score)
        {
            var validationResult = WordMixResultValidator.ValidateResult(letters, currentWordMixRepository.GetCurrent().Letters, dbContext);
            if (validationResult is WordMixValidationError error)
            {
                logger.Error($"Validation error for board {letters} for {person}: {error.message}", person);
                return;
            }

            var successResult = (WordMixValidationSuccess)validationResult;
            if (score != successResult.score)
            {
                logger.Error($"Score mismatch for {person}: expected {score} but was {successResult.score}", person);
                score = successResult.score;
            }

            UpdateResult(DateHelper.DateNow, person, letters, score);
        }

        public WordMixResult? GetForDateAndPerson(DateTime date, string person)
            => dbContext.WordMixResult.FirstOrDefault(x => x.Person == person && x.Date == date);

        public void UpdateResult(DateTime date, string person, string letters, int score)
        {
            var existing = GetForDateAndPerson(date, person);

            if (existing != null)
            {
                if (existing.Score > score)
                {
                    return;
                }

                existing.Board = letters;
                existing.Score = score;
            }
            else
            {
                existing = new WordMixResult
                {
                    Date = date,
                    Person = person,
                    Board = letters,
                    Score = score
                };

                dbContext.WordMixResult.Add(existing);
            }

            dbContext.SaveChanges();
            logger.Information($"User {person} set a new word mix score of {score}.", person);
        }
    }
}
