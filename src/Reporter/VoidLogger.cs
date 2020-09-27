using System;
using DirSync.Interface;

namespace DirSync.Reporter
{
    public class VoidLogger : ILogger
    {
        public void Verbose(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(string message)
        {
        }

        public void Error(string message, Exception exception = null)
        {
        }
    }
}