namespace DirSync.Model
{
    public class SyncRunResult
    {
        public SyncRunResult(SyncConfig syncConfig)
        {
            SyncConfig = syncConfig;
        }

        public SyncConfig SyncConfig { get; set; }

        public int TargetAffectedFileCount { get; set; }
    }
}