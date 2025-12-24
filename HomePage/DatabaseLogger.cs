using HomePage.Data;
using HomePage.Model;

namespace HomePage
{
    public class DatabaseLogger(IServiceScopeFactory serviceScopeFactory)
    {
        public void Information(string message, string? person) => Log(LogRowSeverity.Information, message, person);

        public void Warning(string message, string? person) => Log(LogRowSeverity.Warning, message, person);

        public void Error(string message, string? person) => Log(LogRowSeverity.Error, message, person);

        public void Log(LogRowSeverity severity, string message, string? person, string? stackTrace = null)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
