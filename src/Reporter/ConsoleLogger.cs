using System;
using System.Threading.Tasks;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;

namespace DirSync.Reporter
{
    public class ConsoleLogger : ILogger
    {
        public async Task InfoAsync(string message)
        {
            await ConsoleUtil.WriteLineAsync(messages: new ConsoleMessage(message));
        }

        public async Task WarnAsync(string message)
        {
            await ConsoleUtil.WriteLineAsync(messages: new ConsoleMessage(message));
        }

        public async Task ErrorAsync(string message, Exception exception = null)
        {
            await ConsoleUtil.WriteLineAsync(messages: new ConsoleMessage(message));
            if (exception != null)
            {
                await ConsoleUtil.WriteLineAsync(messages: new ConsoleMessage(exception.ToString()));
            }
        }
    }
}
