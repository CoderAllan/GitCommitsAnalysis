using System.IO;
using SourceCodeAnalysis.Interfaces;

namespace SourceCodeAnalysis
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
    }
}
