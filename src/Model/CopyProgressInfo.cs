using System;

namespace DirSync.Model
{
    public class CopyProgressInfo
    {
        public CopyProgressInfo(long totalBytes, long copiedBytes)
        {
            TotalBytes = totalBytes;
            CopiedBytes = copiedBytes;
        }

        public long TotalBytes { get; set; }

        public long CopiedBytes { get; set; }

        public long CurrentBlockSize { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public double CopiedRatesInBytes
        {
            get
            {
                if (ElapsedMilliseconds <= 0)
                {
                    return 0;
                }

                return Convert.ToDouble(CurrentBlockSize / ElapsedMilliseconds);
            }
        }
    }
}
