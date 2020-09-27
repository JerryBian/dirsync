namespace DirSync.Model
{
    public class CleanupRunResult
    {
        public CleanupRunResult(SyncConfig syncConfig)
        {
            SyncConfig = syncConfig;
        }

        public SyncConfig SyncConfig { get; set; }

        public int TargetAffectedFileCount { get; set; }
    }
}