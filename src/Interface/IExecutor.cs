using System;
using System.Threading.Tasks;

namespace DirSync.Interface
{
    public interface IExecutor
    {
        ILogger Logger { get; set; }

        Type ProgressBarType { get; set; }

        Task ExecuteAsync();
    }
}