using System;
using System.Threading.Tasks;

namespace DirSync.Interface
{
    public interface ILogger
    {
        Task InfoAsync(string message);

        Task WarnAsync(string message);

        Task ErrorAsync(string message, Exception exception = null);
    }
}
