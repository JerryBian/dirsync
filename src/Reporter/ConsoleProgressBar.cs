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
        private ConsoleMessageResult _previousMessageResult;

        public async Task TickAsync(TimeSpan elapsed, long copiedBytes, long totalBytes, double ratesInBytesPerMs)
        {
            //if (Console.CursorLeft != _previousMessageResult.EndCursorLeft ||
            //    Console.CursorTop != _previousMessageResult.EndCursorTop)
            //{
            //    await ConsoleUtil.ClearLinesAsync(_previousMessageResult.EndCursorTop, Console.CursorTop);
            //}

            await ConsoleUtil.ClearLinesAsync(_previousMessageResult.EndCursorTop, Console.CursorTop);

            var message1 = new ConsoleMessage(elapsed.ToString(@"hh\:mm\:ss"), ConsoleColor.DarkGreen);
            var message2 = new ConsoleMessage($"[{copiedBytes/(double)totalBytes:P1}]", ConsoleColor.DarkBlue);
            var message3 = new ConsoleMessage($"({ByteSize.FromBytes(copiedBytes)}/{ByteSize.FromBytes(totalBytes)}, {ByteSize.FromBytes(ratesInBytesPerMs * 1000)}/s)", ConsoleColor.DarkMagenta);
            _previousMessageResult = await ConsoleUtil.WriteLineAsync(messages: new[] {message1, message2, message3});
        }

        public async Task InitAsync(string message)
        {
            _previousMessageResult = await ConsoleUtil.WriteLineAsync(messages:new ConsoleMessage(message));
        }
    }
}
