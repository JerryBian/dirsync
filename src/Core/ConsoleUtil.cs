using System;
using System.Threading.Tasks;
using DirSync.Model;

namespace DirSync.Core
{
    public class ConsoleUtil
    {
        public static async Task ClearLinesAsync(int startCursorTop, int endCursorTop, bool stdErr = false)
        {
            for (var i = startCursorTop; i <= endCursorTop; i++)
            {
                SetCursorPosition(0, i);
                if (stdErr)
                {
                    await Console.Error.WriteAsync(new string(' ', Math.Max(Console.BufferWidth, Console.WindowWidth)));
                }
                else
                {
                    await Console.Out.WriteAsync(new string(' ', Math.Max(Console.BufferWidth, Console.WindowWidth)));
                }
            }

            SetCursorPosition(0, startCursorTop);
        }

        public static void SetCursorPosition(int cursorLeft, int cursorTop)
        {
            Console.SetCursorPosition(cursorLeft, cursorTop);
        }

        public static async Task<ConsoleMessageResult> WriteAsync(
            bool stdErr = false,
            params ConsoleMessage[] messages)
        {
            var result = new ConsoleMessageResult
            {
                StartCursorLeft = Console.CursorLeft,
                StartCursorTop = Console.CursorTop
            };

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

            result.EndCursorLeft = Console.CursorLeft;
            result.EndCursorTop = Console.CursorTop;
            return result;
        }

        public static async Task<ConsoleMessageResult> WriteLineAsync(
            bool stdErr = false,
            params ConsoleMessage[] messages)
        {
            var result = await WriteAsync(stdErr, messages);
            if (stdErr)
            {
                await Console.Error.WriteAsync(Environment.NewLine);
            }
            else
            {
                await Console.Out.WriteAsync(Environment.NewLine);
            }

            result.EndCursorLeft = 0;
            result.EndCursorTop += 1;
            return result;
        }
    }
}
