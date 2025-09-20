using Microsoft.AspNetCore.Diagnostics;

namespace HomePage
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<CustomExceptionHandler> logger;
        private const string Path = "Database/Errors.txt";
        public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            AppendError(exception.Message);
            return ValueTask.FromResult(false);
        }

        public static void AppendError(string error)
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }

            File.AppendAllLines(Path, [error]);
        }

        public static IEnumerable<string> GetErrors()
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
                return [];
            }

            return File.ReadAllLines(Path).Reverse();
        }
    }
}
