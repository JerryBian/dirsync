using System;

namespace DirSync.Model
{
    public class ExecutorResult
    {
        public ExecutorResult(string src, string target)
        {
            Src = src;
            Target = target;
        }

        public string Src { get; set; }

        public string Target { get; set; }

        public int SrcDirCount { get; set; }

        public int SrcFileCount { get; set; }

        public int TargetAffectedFileCount { get; set; }

        public bool Succeed { get; set; }

        public TimeSpan Took { get; set; }

        public Exception Error { get; set; }
    }
}
