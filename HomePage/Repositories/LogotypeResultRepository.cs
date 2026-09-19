using HomePage.Data;
using HomePage.LogoGame;

namespace HomePage.Repositories
{
    public class LogotypeResultRepository(AppDbContext dbContext, LogotypeRepository logotypeRepository)
    {
        public (LogotypeDayResult? current, List<LogotypeDayResult> previous) GetResultsByDay()
        {
            var allLogos = logotypeRepository.LoadLogos().ToDictionary(x => x.Id, x => x);
            var dayLogos = dbContext.DayLogo.ToDictionary(x => x.Date, x => x);
            LogotypeDayResult? current = null;
            var previous = new List<LogotypeDayResult>();
            var allResults = dbContext.LogotypeResults
                .ToList()
                .GroupBy(x => x.Date)
                .OrderByDescending(x => x.Key)
                .ToList();

            if (allResults.Count == 0)
            {
                return (null, []);
            }

            if (allResults.First().Key == DateHelper.DateNow)
            {
                current = DayResultFromGrouping(allResults[0]);
                allResults.RemoveAt(0);
            }

            foreach (var dateGrouping in allResults)
            {
                previous.Add(DayResultFromGrouping(dateGrouping));
            }

            return (current, previous);

            LogotypeDayResult DayResultFromGrouping(IGrouping<DateTime, LogotypeResult> grouping)
            {
                return new LogotypeDayResult
                {
                    Date = grouping.Key,
                    Results = [.. grouping],
                    LogotypeMetadata = allLogos[dayLogos[grouping.Key].LogoMetadataPath]
                };
            }
        }

        public LogotypeResult GetResultForPerson(DateTime date, string userName)
        {
            var existingResult = dbContext.LogotypeResults.FirstOrDefault(x => x.Date == date && x.UserName == userName);
            if (existingResult == null)
            {
                existingResult = new LogotypeResult
                {
                    Answer = "",
                    Date = date,
                    UserName = userName,
                    HintsUsed = 0,
                    IsCorrect = false
                };
                dbContext.LogotypeResults.Add(existingResult);
                dbContext.SaveChanges();
            }

            return existingResult;
        }

        public string? UseHint(string userName, DateTime date)
        {
            var result = GetResultForPerson(date, userName);
            var dayMetadata = GetDayLogoMetadata(date)!;
            var hintsAllowed = dayMetadata.HintsAvailable;
            var answer = dayMetadata.Answers.First();

            if (result.HintsUsed >= hintsAllowed || result.IsCorrect)
            {
                return null;
            }

            result.HintsUsed++;
            dbContext.SaveChanges();
            var lettersToShow = answer[..result.HintsUsed];
            return lettersToShow;
        }

        public LogotypeMetadata? GetDayLogoMetadata(DateTime date)
        {
            var dayLogo = dbContext.DayLogo.FirstOrDefault(x => x.Date == date);
            if (dayLogo == null)
            {
                return null;
            }

            return logotypeRepository.LoadLogoFromId(dayLogo.LogoMetadataPath);
        }

        public void SetDayLogoMetadata(DateTime date, string metadataId)
        {
            var existing = dbContext.DayLogo.FirstOrDefault(x => x.Date == date);
            if (existing == null)
            {
                existing = new DayLogo
                {
                    Date = date,
                    LogoMetadataPath = metadataId
                };
                dbContext.DayLogo.Add(existing);
            }
            else
            {
                existing.LogoMetadataPath = metadataId;
            }

            dbContext.SaveChanges();
        }

        public bool PerformGuess(string userName, string guess, DateTime date)
        {
            var result = GetResultForPerson(date, userName);
            var correctAnswers = GetDayLogoMetadata(date)!.Answers;

            if (!guess.StartsWith(correctAnswers.First()[..result.HintsUsed]))
            {
                return false;
            }

            result.Answer = guess;

            if (GuessHandler.IsCorrect(guess, correctAnswers))
            {
                result.IsCorrect = true;
                dbContext.SaveChanges();
                return true;
            }

            dbContext.SaveChanges();
            return false;
        }

        public bool MayChangeDayLogo(DateTime date)
        {
            if (DateHelper.IsInPast(date))
            {
                return false;
            }

            if (dbContext.LogotypeResults.Where(x => x.Date == date).Any(x => x.IsCorrect))
            {
                return false;
            }

            return true;
        }
    }
}
