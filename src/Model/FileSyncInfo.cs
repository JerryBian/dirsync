namespace DirSync.Model
{
    public class FileSyncInfo
    {
        public FileSyncInfo(string src, string target)
        {
            Src = src;
            Target = target;
        }

        public bool Overwrite { get; set; }

        public string Src { get; set; }

        public string Target { get; set; }
    }
}