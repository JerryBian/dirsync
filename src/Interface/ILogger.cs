using System;

namespace DirSync.Interface
{
    public interface ILogger
    {
        void Verbose(string message);

        void Info(string message);

        void Warn(string message);

        void Error(string message, Exception exception = null);
    }
}