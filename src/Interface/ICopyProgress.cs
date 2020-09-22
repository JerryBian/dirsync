using System;
using System.Threading.Tasks;

namespace DirSync.Interface
{
    public interface ICopyProgress
    {
        Task CompleteAsync(string message, TimeSpan elapsed, double ratesInBytesPerMs);

        Task InitAsync(string message);
    }
}