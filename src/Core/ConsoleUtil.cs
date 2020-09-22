using System;
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
            foreach (var consoleMessage in messages)
            {
                if (consoleMessage.BackgroundColor.HasValue)
                {
                    Console.BackgroundColor = consoleMessage.BackgroundColor.Value;
                }

                if (consoleMessage.ForegroundColor.HasValue)
                {
                    Console.ForegroundColor = consoleMessage.ForegroundColor.Value;
                }

                if (stdErr)
                {
                    await Console.Error.WriteAsync(consoleMessage.Message);
                }
                else
                {
                    await Console.Out.WriteAsync(consoleMessage.Message);
                }

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
            if (stdErr)
            {
                await Console.Error.WriteAsync(Environment.NewLine);
            }
            else
            {
                await Console.Out.WriteAsync(Environment.NewLine);
            }
        }
    }
}