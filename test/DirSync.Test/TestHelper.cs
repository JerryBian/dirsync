using System.IO;
using System.Threading.Tasks;

namespace DirSync.Test
{
    public static class TestHelper
    {
        public static string GetRandomFolderPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetFolderPath(string parentFolder, string folderName = "")
        {
            var path = Path.Combine(parentFolder,
                string.IsNullOrEmpty(folderName) ? Path.GetRandomFileName() : folderName);
            Directory.CreateDirectory(path);
            return path;
        }

        public static async Task CreateRandomFileAsync(string folderPath, int startFile, int endFile, byte[] content)
        {
            Directory.CreateDirectory(folderPath);
            for (var i = startFile; i < endFile; i++)
            {
                var fullPath = Path.Combine(folderPath, i + ".bin");
                await File.WriteAllBytesAsync(fullPath, content);
            }
        }

        public static void DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
    }
}