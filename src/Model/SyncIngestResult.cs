namespace DirSync.Model
{
    public class SyncIngestResult
    {
        public SyncIngestResult(SyncConfig syncConfig)
        {
            SyncConfig = syncConfig;
        }

        public SyncConfig SyncConfig { get; set; }

        public int SourceDirCount { get; set; }

        public int SourceFileCount { get; set; }
    }
}