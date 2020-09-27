using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DirSync.Interface;
using DirSync.Model;

namespace DirSync.Service
{
    public class CleanupService
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ConcurrentQueue<CleanupInfo> _cleanupQueue;
        private readonly ILogger _logger;
        private readonly Options _options;

        private bool _markAsCompleted;

        public CleanupService(
            ILogger logger,
            Options options,
            CancellationToken cancellationToken)
        {
            _logger = logger;
            _options = options;
            _cancellationToken = cancellationToken;
            _cleanupQueue = new ConcurrentQueue<CleanupInfo>();
        }

        public void MarkAsCompleted()
        {
            _markAsCompleted = true;
        }

        public async Task<CleanupIngestResult> IngestAsync(SyncConfig syncConfig)
        {
            var result = new CleanupIngestResult(syncConfig);
            await Task.Run(() => { IngestCore(result, syncConfig.Source, syncConfig.Target); }, _cancellationToken);
            return result;
        }

        public async Task<List<CleanupRunResult>> RunAsync()
        {
            return await Task.Run(async () =>
            {
                var result = new ConcurrentDictionary<SyncConfig, CleanupRunResult>();
                while (true)
                {
                    if (!_cleanupQueue.TryDequeue(out var cleanupInfo))
                    {
                        if (_markAsCompleted)
                        {
                            return result.Values.ToList();
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1), _cancellationToken);
                        continue;
                    }

                    try
                    {
                        var currentResult = result.GetOrAdd(cleanupInfo.SyncConfig, c => new CleanupRunResult(c));
                        RunCore(cleanupInfo, currentResult);
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cleanup {cleanupInfo.FullPath} failed.", ex);
                    }
                }
            }, _cancellationToken);
        }

        private void RunCore(CleanupInfo cleanupInfo, CleanupRunResult result)
        {
            if (cleanupInfo.IsFile)
            {
                if (File.Exists(cleanupInfo.FullPath))
                {
                    File.Delete(cleanupInfo.FullPath);
                }

                result.TargetAffectedFileCount++;
            }
            else
            {
                Directory.Delete(cleanupInfo.FullPath, true);
            }
        }

        private void IngestCore(CleanupIngestResult result, string src, string target)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            foreach (var dir in Directory.EnumerateDirectories(target, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var srcPath = Path.Combine(src, new DirectoryInfo(dir).Name);
                if (!Directory.Exists(srcPath))
                {
                    _cleanupQueue.Enqueue(new CleanupInfo(result.SyncConfig)
                    {
                        FullPath = dir,
                        IsFile = false
                    });
                }

                IngestCore(result, srcPath, dir);
            }

            foreach (var file in Directory.EnumerateFiles(target, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var srcPath = Path.Combine(src, Path.GetFileName(file));
                if (!File.Exists(srcPath))
                {
                    _cleanupQueue.Enqueue(new CleanupInfo(result.SyncConfig)
                    {
                        FullPath = file,
                        IsFile = true
                    });
                }
            }
        }
    }
}