using System;
using System.Threading.Tasks;
using DirSync.Model;

namespace DirSync.Interface
{
    public interface IExecutor
    {
        ILogger Logger { get; set; }

        Type ProgressBarType { get; set; }

        Task<ExecutorResult> ExecuteAsync();
    }
}