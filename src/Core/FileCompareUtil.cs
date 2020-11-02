using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DirSync.Core
{
    public class FileCompareUtil
    {
        private static readonly int MaxBytesScan = 1024;

        public static async Task<bool> ExactlySameAsync(
            string source,
            string target,
            CancellationToken cancellationToken = default)
        {
            // The path have to be exactly same, as for *nix systems path is case sensitive.
            if (string.Equals(source, target))
            {
                return false;
            }

            var fileInfo1 = new FileInfo(source);
            var fileInfo2 = new FileInfo(target);
            if (fileInfo1.Length != fileInfo2.Length)
            {
                return false;
            }

            var iterations = (int)Math.Ceiling((double)fileInfo1.Length / MaxBytesScan);
            await using var f1 = fileInfo1.OpenRead();
            await using var f2 = fileInfo2.OpenRead();
            var maxBytesScan = fileInfo1.Length < MaxBytesScan ? (int)fileInfo1.Length : MaxBytesScan;
            var first = new byte[maxBytesScan];
            var second = new byte[maxBytesScan];

            for (var i = 0; i < iterations; i++)
            {
                await Task.WhenAll(
                    f1.ReadAsync(first, 0, maxBytesScan, cancellationToken),
                    f2.ReadAsync(second, 0, maxBytesScan, cancellationToken));

                if (!AreBytesEqual(first, second))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreBytesEqual(ReadOnlySpan<byte> b1, ReadOnlySpan<byte> b2)
        {
            return b1.SequenceEqual(b2);
        }
    }
}