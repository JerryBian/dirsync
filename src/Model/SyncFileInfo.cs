namespace DirSync.Model
{
    public class SyncFileInfo
    {
        public SyncFileInfo(SyncConfig syncConfig, string sourceFile, string targetFile)
        {
            SyncConfig = syncConfig;
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }

        public bool Overwrite { get; set; }

        public string SourceFile { get; set; }

        public string TargetFile { get; set; }

        public SyncConfig SyncConfig { get; set; }
    }
}