using System;
using System.IO;
using System.Threading.Tasks;
using DirSync.Model;

namespace DirSync.Core
{
    public class ConsoleUtil
    {
        public static async Task WriteAsync(
            bool stdErr = false,
            params ConsoleMessage[] messages)
        {
            var writer = stdErr ? Console.Error : Console.Out;
            foreach (var consoleMessage in messages)
            {
                await WriteTimestampAsync(writer, consoleMessage.Message, consoleMessage.Timestamp);
                if (consoleMessage.BackgroundColor.HasValue)
                {
                    Console.BackgroundColor = consoleMessage.BackgroundColor.Value;
                }

                if (consoleMessage.ForegroundColor.HasValue)
                {
                    Console.ForegroundColor = consoleMessage.ForegroundColor.Value;
                }

                await writer.WriteAsync(consoleMessage.Message);

                if (consoleMessage.BackgroundColor.HasValue ||
                    consoleMessage.ForegroundColor.HasValue)
                {
                    Console.ResetColor();
                }
            }
        }

        public static async Task WriteLineAsync(
            bool stdErr = false,
            params ConsoleMessage[] messages)
        {
            await WriteAsync(stdErr, messages);

            var writer = stdErr ? Console.Error : Console.Out;
            await writer.WriteAsync(Environment.NewLine);
        }

        private static async Task WriteTimestampAsync(TextWriter writer, string message, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            await writer.WriteAsync($"[{timestamp:dd/MM HH:mm:ss}]");
            Console.ResetColor();
            await writer.WriteAsync(" ");
        }
    }
}