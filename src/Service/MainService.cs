using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DirSync.Interface;
using DirSync.Model;
using DirSync.Reporter;

namespace DirSync.Service
{
    public class MainService
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Options _options;

        public MainService(Options options, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _options = options;
        }

        public ILogger Logger { get; set; } = new VoidLogger();

        public Type ProgressBarType { get; set; } = typeof(VoidSyncProgress);

        public async Task<MainResult> RunAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new MainResult();
            if (!string.IsNullOrEmpty(_options.Config) &&
                !string.IsNullOrWhiteSpace(_options.Config))
            {
                if (!await ParseConfigFileAsync())
                {
                    return result;
                }
            }

            if (!_options.SyncConfigs.Any())
            {
                if (string.IsNullOrEmpty(_options.SourceDir) ||
                    string.IsNullOrWhiteSpace(_options.SourceDir))
                {
                    await Logger.ErrorAsync("Missing source directory, please see --help.");
                    return result;
                }

                if (!Directory.Exists(_options.SourceDir))
                {
                    await Logger.ErrorAsync($"Source directory doesn't exist: {_options.SourceDir}.");
                    return result;
                }

                if (string.IsNullOrEmpty(_options.TargetDir) ||
                    string.IsNullOrWhiteSpace(_options.TargetDir))
                {
                    await Logger.ErrorAsync("Missing target directory, please see --help.");
                    return result;
                }

                _options.SyncConfigs.Add(new SyncConfig {Source = _options.SourceDir, Target = _options.TargetDir});
            }

            var syncIngestTasks = new List<Task<SyncIngestResult>>();
            var cleanupIngestTasks = new List<Task<CleanupIngestResult>>();

            var syncService = new SyncService(_options, Logger, ProgressBarType, _cancellationToken);
            var cleanupService = new CleanupService(Logger, _options, _cancellationToken);
            var syncIngestResults = new List<SyncIngestResult>();
            var cleanupIngestResults = new List<CleanupIngestResult>();


            foreach (var item in _options.SyncConfigs)
            {
                syncIngestTasks.Add(syncService.IngestAsync(item));
                if (_options.Cleanup)
                {
                    cleanupIngestTasks.Add(cleanupService.IngestAsync(item));
                }
            }

            var syncIngestCompletedTask = Task.WhenAll(syncIngestTasks)
                .ContinueWith(t => syncService.MarkAsCompleted(), _cancellationToken);
            var cleanupIngestCompletedTask = Task.WhenAll(cleanupIngestTasks)
                .ContinueWith(t => cleanupService.MarkAsCompleted(), _cancellationToken);
            var syncRunTask = syncService.RunAsync();
            var cleanupRunTask = cleanupService.RunAsync();
            await Task.WhenAll(syncIngestCompletedTask, cleanupIngestCompletedTask, syncRunTask, cleanupRunTask);

            foreach (var syncIngestTask in syncIngestTasks)
            {
                syncIngestResults.Add(await syncIngestTask);
            }

            foreach (var cleanupIngestTask in cleanupIngestTasks)
            {
                cleanupIngestResults.Add(await cleanupIngestTask);
            }

            var syncRunResults = await syncRunTask;
            var cleanupRunResults = await cleanupRunTask;
            result.Configs = _options.SyncConfigs;
            result.AggregateSyncIngestResults(syncIngestResults);
            result.AggregateCleanupIngestResults(cleanupIngestResults);
            result.AggregateRunResults(syncRunResults, cleanupRunResults);

            result.Succeed = true;
            stopwatch.Stop();
            await Logger.InfoAsync($"All done. Elapsed: {stopwatch.Elapsed:hh\\:mm\\:ss}.");
            return result;
        }

        private async Task<bool> ParseConfigFileAsync()
        {
            // supplied syncConfig file
            if (!File.Exists(_options.Config))
            {
                await Logger.ErrorAsync($"SyncConfig file not exists: {_options.Config}");
                return false;
            }

            try
            {
                await using var fs = File.OpenRead(_options.Config);
                var configs =
                    await JsonSerializer.DeserializeAsync<List<SyncConfig>>(fs,
                        cancellationToken: _cancellationToken);
                if (!configs.Any())
                {
                    await Logger.ErrorAsync($"Empty content in syncConfig: {_options.Config}");
                    return false;
                }

                foreach (var config in configs)
                {
                    if (_options.SyncConfigs.Exists(m => m == config))
                    {
                        await Logger.WarnAsync(
                            $"Found duplicate syncConfig. Src={config.Source}, Target={config.Target}. Skip.");
                        continue;
                    }

                    if (_options.SyncConfigs.Exists(m => m.Target == config.Target) && _options.Cleanup)
                    {
                        await Logger.ErrorAsync(
                            $"Multiple syncConfig for same target detected: {config.Target}, -c or --cleanup is not supported.");
                        return false;
                    }

                    if (!Directory.Exists(config.Source))
                    {
                        await Logger.ErrorAsync($"Source directory doesn't exist: {config.Source}. Skip.");
                        continue;
                    }

                    _options.SyncConfigs.Add(config);
                }
            }
            catch (Exception ex)
            {
                await Logger.ErrorAsync($"Parsing syncConfig file failed: {_options.Config}", ex);
                return false;
            }

            return _options.SyncConfigs.Any();
        }
    }
}