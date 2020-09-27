using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;

namespace DirSync.Reporter
{
    public class ConsoleLogger : ILogger, IAsyncDisposable
    {
        private bool _markAsCompleted;

        private readonly Task _messageTask;
        private readonly LogLevel _minLogLevel;
        private readonly ConcurrentQueue<ConsoleMessage> _messageQueue;

        public ConsoleLogger(LogLevel minLogLevel, CancellationToken cancellationToken = default)
        {
            _minLogLevel = minLogLevel;
            _messageQueue = new ConcurrentQueue<ConsoleMessage>();
            _messageTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (!_messageQueue.TryDequeue(out var message) || message == null)
                    {
                        if (_markAsCompleted)
                        {
                            return;
                        }

                        await Task.Delay(10);
                        continue;
                    }

                    await ProcessMessageAsync(message);
                }
            }, cancellationToken);
        }

        private async Task ProcessMessageAsync(ConsoleMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Verbose:
                    await ConsoleUtil.WriteLineAsync(messages: message);
                    break;
                case LogLevel.Info:
                    await ConsoleUtil.WriteLineAsync(messages: message);
                    break;
                case LogLevel.Warn:
                    await ConsoleUtil.WriteLineAsync(messages: message);
                    break;
                case LogLevel.Error:
                    await ConsoleUtil.WriteLineAsync(messages: message);
                    break;
                default:
                    throw new NotSupportedException($"Log level({message.Level} is not supported.)");
            }
        }

        public void Verbose(string message)
        {
            if (_minLogLevel <= LogLevel.Verbose)
            {
                _messageQueue.Enqueue(new ConsoleMessage(message, LogLevel.Verbose, ConsoleColor.Gray, ConsoleColor.Black));
            }
        }

        public void Info(string message)
        {
            if (_minLogLevel <= LogLevel.Info)
            {
                _messageQueue.Enqueue(new ConsoleMessage(message, LogLevel.Info));
            }
        }

        public void Warn(string message)
        {
            if (_minLogLevel <= LogLevel.Warn)
            {
                _messageQueue.Enqueue(new ConsoleMessage(message, LogLevel.Warn, ConsoleColor.Yellow, ConsoleColor.Black));
            }
        }

        public void Error(string message, Exception exception = null)
        {
            if (_minLogLevel <= LogLevel.Error)
            {
                message = exception == null ? message : message + Environment.NewLine + exception;
                _messageQueue.Enqueue(new ConsoleMessage(message, LogLevel.Error, ConsoleColor.Black, ConsoleColor.Red));
            }
        }

        public async ValueTask DisposeAsync()
        {
            _markAsCompleted = true;
            await _messageTask;
        }
    }
}