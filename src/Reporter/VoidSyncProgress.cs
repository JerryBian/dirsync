using DirSync.Interface;
using System;
using System.Threading.Tasks;

namespace DirSync.Reporter
{
    public class VoidSyncProgress : ISyncProgress
    {
        public async Task CompleteAsync(string message, TimeSpan elapsed, double ratesInBytesPerMs)
        {
            await Task.CompletedTask;
        }

        public async Task InitAsync(string message)
        {
            await Task.CompletedTask;
        }
    }
}