using System;

namespace DirSync.Model
{
    public class ConsoleMessage
    {
        public ConsoleMessage(string message, LogLevel logLevel, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null)
        {
            Message = message;
            Level = logLevel;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public string Message { get; set; }

        public ConsoleColor? ForegroundColor { get; set; }

        public ConsoleColor? BackgroundColor { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public LogLevel Level { get; set; }
    }
}