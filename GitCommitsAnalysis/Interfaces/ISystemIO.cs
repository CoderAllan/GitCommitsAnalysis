using System.IO;

namespace GitCommitsAnalysis.Interfaces
{
    public interface ISystemIO
    {
        string ReadFileContent(string filename);
        void WriteAllText(string filename, string contents);
        bool FileExists(string filename);
        string GetPathWitoutExtension(string filename);
        string GetExtension(string filename);
        FileInfo FileInfo(string filename);
    }
}
