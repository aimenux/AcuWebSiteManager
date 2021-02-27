using System.IO;

namespace Lib.Helpers
{
    public class FileHelper : IFileHelper
    {
        public void ModifyFile(string filepath, string oldString, string newString)
        {
            var copyFilepath = GenerateCopyFilePath(filepath);
            File.Copy(filepath, copyFilepath, true);
            var content = File.ReadAllText(filepath).Replace(oldString, newString);
            File.WriteAllText(filepath, content);
        }

        private static string GenerateCopyFilePath(string filepath)
        {
            const string prefix = "__Copy";
            var filename = Path.GetFileName(filepath);
            var directoryName = Path.GetDirectoryName(filepath) ?? "/";
            return Path.Combine(directoryName, $"{prefix}{filename}");
        }
    }
}