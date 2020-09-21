using System;
using System.Threading.Tasks;
using ByteSizeLib;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;

namespace DirSync.Reporter
{
    public class ConsoleProgressBar : IProgressBar
    {
        private ConsoleMessageResult _initMessageResult;

        public async Task TickAsync(TimeSpan elapsed, long copiedBytes, long totalBytes, double ratesInBytesPerMs)
        {
            if (Console.CursorLeft != _initMessageResult.EndCursorLeft ||
                Console.CursorTop != _initMessageResult.EndCursorTop)
            {
                await ConsoleUtil.ClearLinesAsync(_initMessageResult.EndCursorTop, Console.CursorTop);
            }

            var message1 = new ConsoleMessage(elapsed.ToString(@"hh\:mm\:ss"), ConsoleColor.DarkGreen);
            var message2 = new ConsoleMessage($"[{copiedBytes/(double)totalBytes:P1}]", ConsoleColor.DarkBlue);
            var message3 = new ConsoleMessage($"({ByteSize.FromBytes(copiedBytes)}/{ByteSize.FromBytes(totalBytes)}, {ByteSize.FromBytes(ratesInBytesPerMs * 1000)}/s)", ConsoleColor.DarkMagenta);
            await ConsoleUtil.WriteLineAsync(messages: new[] {message1, message2, message3});
        }

        public async Task InitAsync(string message)
        {
            _initMessageResult = await ConsoleUtil.WriteLineAsync(messages:new ConsoleMessage(message));
        }
    }
}
