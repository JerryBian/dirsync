using System;
using System.Threading.Tasks;
using ByteSizeLib;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;

namespace DirSync.Reporter
{
    public class ConsoleSyncProgress : ISyncProgress
    {
        public async Task CompleteAsync(string message, TimeSpan elapsed, double ratesInBytesPerMs)
        {
            var consoleMessage = new ConsoleMessage(
                $"{message}. Elapsed: {elapsed:hh\\:mm\\:ss}, average speed: {ByteSize.FromBytes(ratesInBytesPerMs * 1000)}/s",
                ConsoleColor.DarkGreen);
            await ConsoleUtil.WriteLineAsync(messages: consoleMessage);
        }

        public async Task InitAsync(string message)
        {
            await ConsoleUtil.WriteLineAsync(messages: new ConsoleMessage(message));
        }
    }
}