using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DirSync.Model;

namespace DirSync.Core
{
    public class FileCopyUtil
    {
        private readonly string _src;
        private readonly string _target;
        private readonly bool _overwrite;

        public FileCopyUtil(string src, string target, bool overwrite)
        {
            _src = src;
            _target = target;
            _overwrite = overwrite;
        }

        public long TotalBytes { get; set; }

        public async Task CopyAsync(
            Action<CopyProgressInfo> copyInProgress,
            Action<Exception> copyCompleted,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using (var source = new FileStream(_src, FileMode.Open, FileAccess.Read))
                {
                    TotalBytes = source.Length;
                    if (_overwrite && File.Exists(_target))
                    {
                        File.Delete(_target);
                    }

                    await using var dest = new FileStream(_target, FileMode.CreateNew, FileAccess.Write);
                    var copiedBytes = 0L;
                    int currentBlockSize;
                    var buffer = new byte[1024 * 1024 * 10];

                    while ((currentBlockSize = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        copiedBytes += currentBlockSize;
                        var stopwatch = Stopwatch.StartNew();
                        await dest.WriteAsync(buffer, 0, currentBlockSize, cancellationToken);
                        stopwatch.Stop();
                        var copyProgressInfo = new CopyProgressInfo(TotalBytes, copiedBytes)
                        {
                            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                            CurrentBlockSize = currentBlockSize
                        };
                        copyInProgress(copyProgressInfo);
                        if (TotalBytes > copiedBytes)
                        {
                            var rates = Convert.ToInt32(copyProgressInfo.CopiedRatesInBytes * 1000);
                            buffer = new byte[Math.Min(1024 * 1024 * 10, rates)];
                        }
                    }
                }

                copyCompleted(null);
            }
            catch (Exception ex)
            {
                copyCompleted(ex);
            }
        }
    }
}