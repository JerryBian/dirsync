namespace DirSync.Model
{
    public class CleanupInfo
    {
        public CleanupInfo(SyncConfig syncConfig)
        {
            SyncConfig = syncConfig;
        }

        public SyncConfig SyncConfig { get; set; }

        public bool IsFile { get; set; }

        public string FullPath { get; set; }
    }
}