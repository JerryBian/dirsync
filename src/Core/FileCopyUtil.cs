using System;
using System.IO;
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
            TotalBytes = new FileInfo(src).Length;
        }

        public long TotalBytes { get; set; }

        public async Task CopyAsync(
            Action copyStarted,
            Action<Exception> copyCompleted)
        {
            try
            {
                copyStarted();
                File.Copy(_src, _target, _overwrite);
                copyCompleted(null);
            }
            catch (Exception ex)
            {
                copyCompleted(ex);
            }
        }
    }
}