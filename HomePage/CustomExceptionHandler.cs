using HomePage.Data;
using Microsoft.AspNetCore.Diagnostics;

namespace HomePage
{
    public class CustomExceptionHandler(IServiceProvider services) : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<DatabaseLogger>();

            string? person = null;

            try
            {
                if (httpContext.Session?.IsAvailable == true)
                {
                    person = httpContext.Session.GetString("LoggedInPerson");
                }
            } catch { }

            logger.Log(Model.LogRowSeverity.Error, exception.Message, person, exception.StackTrace);
            return ValueTask.FromResult(false);
        }
    }
}
