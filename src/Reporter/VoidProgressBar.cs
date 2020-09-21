using System;
using System.Threading.Tasks;
using DirSync.Interface;

namespace DirSync.Reporter
{
    public class VoidProgressBar : IProgressBar
    {
        public async Task TickAsync(TimeSpan elapsed, long copiedBytes, long totalBytes, double ratesInBytesPerMs)
        {
            await Task.CompletedTask;
        }

        public async Task InitAsync(string message)
        {
            await Task.CompletedTask;
        }
    }
}
