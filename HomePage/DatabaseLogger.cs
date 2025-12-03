using HomePage.Data;
using HomePage.Model;

namespace HomePage
{
    public class DatabaseLogger(AppDbContext dbContext)
    {
        public void Information(string message, string? person) => Log(LogRowSeverity.Information, message, person);

        public void Warning(string message, string? person) => Log(LogRowSeverity.Warning, message, person);

        public void Error(string message, string? person) => Log(LogRowSeverity.Error, message, person);

        public void Log(LogRowSeverity severity, string message, string? person, string? stackTrace = null)
        {
            dbContext.LogRow.Add(new LogRow
            {
                LogRowSeverity = severity,
                Message = message,
                PersonCause = person,
                LogDate = DateHelper.DateTimeNow,
                StackTrace = stackTrace
            });
            dbContext.SaveChanges();
        }
    }
}
