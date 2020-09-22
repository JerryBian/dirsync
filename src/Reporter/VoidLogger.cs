using System;
using System.Threading.Tasks;
using DirSync.Interface;

namespace DirSync.Reporter
{
    public class VoidLogger : ILogger
    {
        public async Task InfoAsync(string message)
        {
            await Task.CompletedTask;
        }

        public async Task WarnAsync(string message)
        {
            await Task.CompletedTask;
        }

        public async Task ErrorAsync(string message, Exception exception = null)
        {
            await Task.CompletedTask;
        }
    }
}