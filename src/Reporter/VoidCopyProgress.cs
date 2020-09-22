using System;
using System.Threading.Tasks;
using DirSync.Interface;

namespace DirSync.Reporter
{
    public class VoidCopyProgress : ICopyProgress
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