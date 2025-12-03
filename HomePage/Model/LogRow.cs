using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class LogRow
    {
        [Key]
        public Guid Id { get; set; }

        public LogRowSeverity LogRowSeverity { get; set; }

        public DateTime LogDate { get; set; }

        [MaxLength(50)]
        public string? PersonCause { get; set; }

        public required string Message { get; set; }

        public string? StackTrace { get; set; }

        public string TruncatedMessage => Message.Length > 200 ? Message[..200] : Message;
    }

    public enum LogRowSeverity
    {
        Error,
        Warning,
        Information
    }
}
