using HomePage.Achievements;
using HomePage.Data;
using HomePage.LogoGame;

namespace HomePage.Repositories
{
    public class LogotypeResultRepository(AppDbContext dbContext, LogotypeRepository logotypeRepository, 
        AchievementRepository achievementRepository, DatabaseLogger logger)
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

        public List<LogotypeResult> GetResultsForDay(DateTime date) => dbContext.LogotypeResults.Where(x => x.Date == date && x.IsCorrect).ToList();

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
                    IsCorrect = false,
                    CreatedAt = DateHelper.DateTimeNow
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

        public LogotypeMetadata GenerateNewDayLogo()
        {
            var previousDays = dbContext.DayLogo.OrderByDescending(x => x.Date).ToList();
            var seenUsers = new HashSet<string>();
            var seenLogos = new HashSet<string>();
            var users = new List<string>();
            foreach (var day in previousDays)
            {
                var logo = logotypeRepository.LoadLogoFromId(day.LogoMetadataPath);
                seenLogos.Add(logo.Id);
                if (seenUsers.Add(logo.GetCreator))
                {
                    users.Add(logo.GetCreator);
                }
            }

            users.AddRange(dbContext.UserInfo.Select(x => x.UserName).Where(x => !seenUsers.Contains(x)));
            for (var i = users.Count - 1; i >= 0; i--)
            {
                var logo = TryPickLogoFromUser(users[i]);
                if (logo != null)
                {
                    SetDayLogoMetadata(DateHelper.DateNow, logo.Id);
                    return logo;
                }
            }

            var lastLogo = logotypeRepository.LoadLogoFromId(seenLogos.Last());
            SetDayLogoMetadata(DateHelper.DateNow, lastLogo.Id);
            return lastLogo;
            LogotypeMetadata? TryPickLogoFromUser(string userName)
            {
                var userLogos = logotypeRepository.LoadLogos(userName).Where(x => !seenLogos.Contains(x.Id));
                return userLogos.FirstOrDefault();
            }
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
            if (result.IsCorrect)
            {
                return false;
            }

            var correctAnswers = GetDayLogoMetadata(date)!.Answers;
            result.Guesses++;

            if (!guess.StartsWith(correctAnswers.First()[..result.HintsUsed]))
            {
                logger.Information($"{userName} guessed wrong on todays logo with answer {guess}, correct answer was {correctAnswers.First()}");
                dbContext.SaveChanges();
                return false;
            }

            result.Answer = guess;

            if (GuessHandler.IsCorrect(guess, correctAnswers))
            {
                logger.Information($"{userName} guessed correct on todays logo with answer {guess}");
                result.IsCorrect = true;
                result.FinishedAt = DateHelper.DateTimeNow;
                dbContext.SaveChanges();
                if ((DateHelper.DateTimeNow - result.CreatedAt).TotalSeconds <= 10)
                {
                    achievementRepository.EarnAchievement(AchievementRepository.QuickClick, userName);
                }

                if (result.HintsUsed == 0 && result.Guesses == 1 && Enumerable.Range(1, 4).Select(x => date.AddDays(-x)).All(newDate =>
                {
                    var result = dbContext.LogotypeResults.FirstOrDefault(x => x.Date == newDate && x.UserName == userName);
                    return result?.IsCorrect == true && result.Guesses == 1 && result.HintsUsed == 0;
                }))
                {
                    achievementRepository.EarnAchievement(AchievementRepository.Reklamskadad, userName);
                }

                return true;
            }
            else
            {
                logger.Information($"{userName} guessed wrong on todays logo with answer {guess}, correct answer was {correctAnswers.First()}");
            }

            dbContext.SaveChanges();
            return false;
        }

        public bool MayChangeDayLogo(DateTime date, string username)
        {
            if (DateHelper.IsInPast(date))
            {
                return false;
            }

            if (dbContext.LogotypeResults.Where(x => x.Date == date).Any(x => x.IsCorrect))
            {
                return false;
            }

            if ((GetDayLogoMetadata(date)?.GetCreator ?? username) != username)
            {
                return false;
            }
            return true;
        }

        public static IEnumerable<LogotypeResult> OrderResults(IEnumerable<LogotypeResult> results) => results.Where(x => x.IsCorrect).OrderBy(x => x.HintsUsed).ThenBy(x => x.Guesses).ThenBy(x => (x.FinishedAt - x.CreatedAt)?.TotalSeconds);
    }
}
