namespace DirSync.Model
{
    public class CleanupIngestResult
    {
        public CleanupIngestResult(SyncConfig syncConfig)
        {
            SyncConfig = syncConfig;
        }

        public SyncConfig SyncConfig { get; set; }

        public int TargetAffectedFileCount { get; set; }
    }
}