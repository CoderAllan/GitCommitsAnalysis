using System.IO;
using GitCommitsAnalysis.Interfaces;

namespace GitCommitsAnalysis
{
    public class SystemIO : ISystemIO
    {
        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        public string ReadFileContent(string filename)
        {
            return File.ReadAllText(filename);
        }

        public void WriteAllText(string filename, string contents)
        {
            File.WriteAllText(filename, contents);
        }

        public string GetPathWitoutExtension(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename);
        }

        public string GetExtension(string filename)
        {
            return Path.GetExtension(filename);
        }

        public FileInfo FileInfo(string filename)
        {
            return new FileInfo(filename);
        }
    }
}
