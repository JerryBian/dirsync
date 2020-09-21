using System;

namespace DirSync.Model
{
    public class ConsoleMessage
    {
        public ConsoleMessage(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            Message = message;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public string Message { get; set; }

        public ConsoleColor? ForegroundColor { get; set; }

        public ConsoleColor? BackgroundColor { get; set; }
    }
}
