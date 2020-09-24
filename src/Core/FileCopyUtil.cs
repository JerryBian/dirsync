using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirSync.Core
{
    public class FileCopyUtil
    {
        private readonly bool _overwrite;
        private readonly string _src;
        private readonly string _target;

        public FileCopyUtil(string src, string target, bool overwrite)
        {
            _src = src;
            _target = target;
            _overwrite = overwrite;
        }

        public long TotalBytes { get; set; }

        public async Task CopyAsync(
            Action copyStarted,
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

                    await using var dest = new FileStream(_target, FileMode.Create, FileAccess.Write);
                    copyStarted();
                    await source.CopyToAsync(dest, cancellationToken);
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