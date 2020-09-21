using System;
using System.Threading.Tasks;

namespace DirSync.Interface
{
    public interface IProgressBar
    {
        Task TickAsync(TimeSpan elapsed, long copiedBytes, long totalBytes, double ratesInBytesPerMs);

        Task InitAsync(string message);
    }
}
