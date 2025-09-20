using System.IO.Compression;
using System.Text;

namespace HomePage
{
    public class WordMixResultRepository : Repository<WordMixResult>
    {
        public override string FileName => "WordMixResult.txt";

        public void UpdateResultWithValidation(string person, string letters, int score)
        {
            var validationResult = WordMixResultValidator.ValidateResult(letters, new CurrentWordMixRepository().GetCurrent().Letters);
            if (validationResult is WordMixValidationError error)
            {
                CustomExceptionHandler.AppendError($"Validation error for board {letters} for {person}: {error.message}");
                return;
            }

            var successResult = (WordMixValidationSuccess)validationResult;
            if (score != successResult.score)
            {
                CustomExceptionHandler.AppendError($"Score mismatch for {person}: expected {score} but was {successResult.score}");
                score = successResult.score;
            }

            UpdateResult(DateHelper.DateNow, person, letters, score);
        }

        public void UpdateResult(DateTime date, string person, string letters, int score)
        {
            var id = WordMixResult.MakeId(date, person);
            var existing = this.TryGetValue(id);

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
                    Day = DateHelper.ToKey(date),
                    Person = person,
                    Board = letters,
                    Score = score
                };
            }

            this.SaveValue(existing);
        }
    }
}
