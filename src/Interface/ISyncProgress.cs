using System;
using System.Threading.Tasks;

namespace DirSync.Interface
{
    public interface ISyncProgress
    {
        Task CompleteAsync(string message, TimeSpan elapsed, double ratesInBytesPerMs);

        Task InitAsync(string message);
    }
}