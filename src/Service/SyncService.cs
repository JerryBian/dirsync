using ByteSizeLib;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DirSync.Service
{
    public class SyncService
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Type _copyProgress;
        private readonly ILogger _logger;
        private readonly Options _options;
        private readonly ConcurrentQueue<SyncFileInfo> _syncQueue;

        private bool _markAsCompleted;

        public SyncService(
            Options options,
            ILogger logger,
            Type copyProgress,
            CancellationToken cancellationToken = default)
        {
            _logger = logger;
            _options = options;
            _copyProgress = copyProgress;
            _syncQueue = new ConcurrentQueue<SyncFileInfo>();
            _cancellationToken = cancellationToken;
        }

        public async Task<SyncIngestResult> IngestAsync(SyncConfig syncConfig)
        {
            var result = new SyncIngestResult(syncConfig);
            await IngestCoreAsync(syncConfig, syncConfig.Source, syncConfig.Target, result);
            return result;
        }

        public void MarkAsCompleted()
        {
            _markAsCompleted = true;
        }

        public async Task<List<SyncRunResult>> RunAsync()
        {
            return await Task.Run(async () =>
            {
                var result = new ConcurrentDictionary<SyncConfig, SyncRunResult>();
                while (true)
                {
                    if (!_syncQueue.TryDequeue(out var fileSyncInfo))
                    {
                        if (_markAsCompleted)
                        {
                            return result.Values.ToList();
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1), _cancellationToken);
                        continue;
                    }

                    var currentFile = Path.GetRelativePath(fileSyncInfo.SyncConfig.Source, fileSyncInfo.SourceFile);
                    var currentResult = result.GetOrAdd(fileSyncInfo.SyncConfig, c => new SyncRunResult(c));

                    try
                    {
                        await SyncCoreAsync(fileSyncInfo, currentFile, currentResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error while copying {currentFile}.", ex);
                    }
                }
            }, _cancellationToken);
        }

        private async Task SyncCoreAsync(SyncFileInfo syncFileInfo, string currentFile, SyncRunResult result)
        {
            if (!(Activator.CreateInstance(_copyProgress) is ISyncProgress bar))
            {
                throw new ApplicationException($"Invalid progress bar implementation: {_copyProgress}");
            }

            var fileCopyUtil =
                new FileCopyUtil(syncFileInfo.SourceFile, syncFileInfo.TargetFile, syncFileInfo.Overwrite);

            var stopwatch = Stopwatch.StartNew();
            await fileCopyUtil.CopyAsync(
                async () => { await bar.InitAsync($"{currentFile} ({ByteSize.FromBytes(fileCopyUtil.TotalBytes)})"); },
                async e =>
                {
                    if (e != null)
                    {
                        _logger.Error($"Copying {currentFile} failed.", e);
                    }
                    else
                    {
                        await bar.CompleteAsync(
                            "Finished",
                            stopwatch.Elapsed,
                            (double)fileCopyUtil.TotalBytes / stopwatch.ElapsedMilliseconds);
                        result.TargetAffectedFileCount++;
                    }
                });
        }

        private async Task IngestCoreAsync(SyncConfig syncConfig, string src, string target, SyncIngestResult result)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            Directory.CreateDirectory(target);
            foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                result.SourceDirCount++;
                await IngestCoreAsync(syncConfig, dir, Path.Combine(target, new DirectoryInfo(dir).Name), result);
            }

            foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                result.SourceFileCount++;

                var fileName = Path.GetFileName(file);
                if (_options.ExcludePatterns.Value != null && _options.ExcludePatterns.Value.Any())
                {
                    var matched = false;
                    foreach (var excludePattern in _options.ExcludePatterns.Value)
                    {
                        if (excludePattern.IsMatch(fileName))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                    {
                        continue;
                    }
                }

                if (_options.IncludePatterns.Value != null && _options.IncludePatterns.Value.Any())
                {
                    var matched = false;
                    foreach (var includePattern in _options.IncludePatterns.Value)
                    {
                        if (includePattern.IsMatch(fileName))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        continue;
                    }
                }

                var targetFilePath = Path.Combine(target, fileName);
                var syncInfo = new SyncFileInfo(syncConfig, file, targetFilePath);
                if (!File.Exists(targetFilePath))
                {
                    _syncQueue.Enqueue(syncInfo);
                }
                else
                {
                    if (_options.Force)
                    {
                        syncInfo.Overwrite = true;
                        _syncQueue.Enqueue(syncInfo);
                    }
                    else if (_options.Strict)
                    {
                        var exactlySame =
                            await FileCompareUtil.ExactlySameAsync(file, targetFilePath, _cancellationToken);
                        if (!exactlySame)
                        {
                            syncInfo.Overwrite = true;
                            _syncQueue.Enqueue(syncInfo);
                        }
                    }
                }
            }
        }
    }
}