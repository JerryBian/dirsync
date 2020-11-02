#nullable enable
using CommandLine;
using DirSync.Model;
using DirSync.Reporter;
using DirSync.Service;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirSync
{
    internal class Program
    {
        private static bool _cancelEventFired;
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static async Task Main(string[] args)
        {
            //Debugger.Launch();
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
            await parser.ParseArguments<Options>(args).WithParsedAsync(async o =>
            {
                var startTime = DateTime.Now;
                var result = await ExecuteAsync(o);
                var stopTime = DateTime.Now;
                // generate reports
                var sb = new StringBuilder();
                sb.AppendLine("<ul>");
                sb.AppendLine($"<li>Start: {startTime}</li>");
                sb.AppendLine($"<li>End: {stopTime}</li>");
                sb.AppendLine($"<li>Elapsed: {stopTime - startTime}</li>");
                sb.AppendLine($"Succeed: {result.Succeed}");
                if (result.Error != null)
                {
                    sb.AppendLine($"Error: {result.Error}");
                }
                sb.AppendLine("</ul>");
            });
        }

        private static async void TaskSchedulerOnUnobservedTaskException(object? sender,
            UnobservedTaskExceptionEventArgs e)
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

        private static async Task<MainResult> ExecuteAsync(Options options)
        {
            await using var logger = new ConsoleLogger(options.Verbose ? LogLevel.Verbose : LogLevel.Info);
            var executor = new MainService(options, CancellationTokenSource.Token)
            {
                Logger = logger,
                ProgressBarType = typeof(ConsoleSyncProgress)
            };

            return await executor.RunAsync();
        }
    }
}