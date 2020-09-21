using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DirSync.Core;
using DirSync.Interface;
using DirSync.Model;
using DirSync.Reporter;

namespace DirSync.Executor
{
    public class SyncExecutor : IExecutor
    {
        private readonly Options _options;
        private readonly ConcurrentQueue<FileSyncInfo> _filSyncQueue;
        private readonly ConcurrentQueue<CleanupInfo> _cleanupQueue;
        private readonly Task _syncTask;
        private readonly Task _cleanupTask;
        private readonly CancellationToken _cancellationToken;

        private bool _markAsCompleted;
        private int _affectedFiles;
        private int _srcDirCount;
        private int _srcFileCount;

        public SyncExecutor(Options options, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _options = options;
            _filSyncQueue = new ConcurrentQueue<FileSyncInfo>();
            _cleanupQueue = new ConcurrentQueue<CleanupInfo>();
            _syncTask = CreateSyncTask();
            _cleanupTask = CreateCleanupTask();
        }

        private Task CreateCleanupTask()
        {
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    if (!_cleanupQueue.TryDequeue(out var cleanupInfo))
                    {
                        if (_markAsCompleted)
                        {
                            return;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1), _cancellationToken);
                        continue;
                    }

                    if (cleanupInfo.IsFile)
                    {
                        if (File.Exists(cleanupInfo.FullPath))
                        {
                            File.Delete(cleanupInfo.FullPath);
                        }

                        Interlocked.Increment(ref _affectedFiles);
                    }
                    else
                    {
                        Directory.Delete(cleanupInfo.FullPath, true);
                    }
                }
            }, _cancellationToken);

            return task;
        }

        private Task CreateSyncTask()
        {
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    if (!_filSyncQueue.TryDequeue(out var fileSyncInfo))
                    {
                        if (_markAsCompleted)
                        {
                            return;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(1), _cancellationToken);
                        continue;
                    }

                    var currentFile = Path.GetRelativePath(_options.SourceDir, fileSyncInfo.Src);
                    try
                    {
                        if (!(Activator.CreateInstance(ProgressBarType) is IProgressBar bar))
                        {
                            throw new ApplicationException($"Invalid progress bar implementation: {ProgressBarType}");
                        }

                        await bar.InitAsync(currentFile);
                        var fileCopyUtil =
                            new FileCopyUtil(fileSyncInfo.Src, fileSyncInfo.Target, fileSyncInfo.Overwrite);
                        var stopwatch = Stopwatch.StartNew();
                        await fileCopyUtil.CopyAsync(async p =>
                        {
                            await bar.TickAsync(stopwatch.Elapsed, p.CopiedBytes, p.TotalBytes,
                                p.CopiedRatesInBytes);
                        }, async e =>
                        {
                            await bar.TickAsync(
                                stopwatch.Elapsed, 
                                fileCopyUtil.TotalBytes, 
                                fileCopyUtil.TotalBytes,
                                (double)fileCopyUtil.TotalBytes / stopwatch.ElapsedMilliseconds);
                            if (e != null)
                            {
                                await Logger.ErrorAsync($"Error while copying {currentFile}: {e.Message}");
                            }
                            else
                            {
                                Interlocked.Increment(ref _affectedFiles);
                            }
                        }, _cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await Logger.ErrorAsync($"Error while copying {currentFile}: {ex.Message}");
                    }
                    finally
                    {
                        await Logger.InfoAsync(string.Empty);
                    }
                }
            }, _cancellationToken);

            return task;
        }

        private void CleanupTargetDir(string src, string target)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            foreach (var dir in Directory.EnumerateDirectories(target, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var srcPath = Path.Combine(src, new DirectoryInfo(dir).Name);
                if (!Directory.Exists(srcPath))
                {
                    _cleanupQueue.Enqueue(new CleanupInfo
                    {
                        FullPath = dir,
                        IsFile = false
                    });
                }

                CleanupTargetDir(srcPath, dir);
            }

            foreach (var file in Directory.EnumerateFiles(target, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var srcPath = Path.Combine(src, Path.GetFileName(file));
                if (!File.Exists(srcPath))
                {
                    _cleanupQueue.Enqueue(new CleanupInfo
                    {
                        FullPath = file,
                        IsFile = true
                    });
                }
            }
        }

        public ILogger Logger { get; set; } = new VoidLogger();

        public Type ProgressBarType { get; set; } = typeof(VoidProgressBar);

        public async Task<ExecutorResult> ExecuteAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var executorResult = new ExecutorResult(_options.SourceDir, _options.TargetDir);
            try
            {
                await ExecuteCoreAsync(_options.SourceDir, _options.TargetDir, _options.Force, _options.Strict);
                if (_options.Cleanup)
                {
                    CleanupTargetDir(_options.SourceDir, _options.TargetDir);
                }

                _markAsCompleted = true;
                await Task.WhenAll(_syncTask, _cleanupTask);
                executorResult.Succeed = true;
                executorResult.TargetAffectedFileCount = _affectedFiles;
                executorResult.SrcDirCount = _srcDirCount;
                executorResult.SrcFileCount = _srcFileCount;

                if (!_cancellationToken.IsCancellationRequested)
                {
                    await Logger.InfoAsync("Process completed.");
                }
            }
            catch (Exception ex)
            {
                executorResult.Succeed = false;
                executorResult.Error = ex;
                await Logger.ErrorAsync("Process failed", ex);
            }

            stopwatch.Stop();
            executorResult.Took = stopwatch.Elapsed;
            return executorResult;
        }

        private async Task ExecuteCoreAsync(string src, string target, bool overwrite, bool strict)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(target);
            foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                _srcDirCount++;
                await ExecuteCoreAsync(dir, Path.Combine(target, new DirectoryInfo(dir).Name), overwrite, strict);
            }

            foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.TopDirectoryOnly))
            {
                _cancellationToken.ThrowIfCancellationRequested();
                _srcFileCount++;
                var targetFilePath = Path.Combine(target, Path.GetFileName(file));
                var syncInfo = new FileSyncInfo(file, targetFilePath);
                if (!File.Exists(targetFilePath))
                {
                    _filSyncQueue.Enqueue(syncInfo);
                }
                else
                {
                    if (overwrite)
                    {
                        syncInfo.Overwrite = true;
                        _filSyncQueue.Enqueue(syncInfo);
                    }
                    else if (strict)
                    {
                        var exactlySame =
                            await FileCompareUtil.ExactlySameAsync(file, targetFilePath, _cancellationToken);
                        if (!exactlySame)
                        {
                            syncInfo.Overwrite = true;
                            _filSyncQueue.Enqueue(syncInfo);
                        }
                    }
                }
            }
        }
    }
}
