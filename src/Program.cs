#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using DirSync.Executor;
using DirSync.Reporter;

namespace DirSync
{
    internal class Program
    {
        private static bool _cancelEventFired;
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static async Task Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            Process.GetCurrentProcess().Exited += OnExited;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            var parser = new Parser(c =>
            {
                c.AutoHelp = true;
                c.AutoVersion = true;
                c.CaseInsensitiveEnumValues = true;
                c.CaseSensitive = false;
                c.EnableDashDash = true;
                c.HelpWriter = Console.Error;
                c.IgnoreUnknownArguments = true;
            });
            await parser.ParseArguments<Options>(args).WithParsedAsync(ExecuteAsync);
        }

        private static async void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            await Console.Error.WriteLineAsync($"Unobserved error: {e.Exception?.Message}");
            e.SetObserved();
            Cancel();
        }

        private static void CurrentDomainOnProcessExit(object? sender, EventArgs e)
        {
            Cancel();
        }

        private static void OnExited(object? sender, EventArgs e)
        {
            Cancel();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cancel();
            Console.WriteLine();
            Console.WriteLine("Cancelled.");
        }

        private static void Cancel()
        {
            if (_cancelEventFired)
            {
                return;
            }

            _cancelEventFired = true;
            CancellationTokenSource.Cancel();
        }

        private static async Task ExecuteAsync(Options options)
        {
            var executor = new SyncExecutor(options, CancellationTokenSource.Token)
            {
                Logger = new ConsoleLogger(),
                ProgressBarType = typeof(ConsoleProgressBar)
            };
            await executor.ExecuteAsync();
        }
    }
}
