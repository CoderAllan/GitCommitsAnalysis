namespace SourceCodeAnalysis.Interfaces
{
    public interface IFileHandling
    {
        string ReadFileContent(string filename);
        void WriteAllText(string filename, string contents);
        bool FileExists(string filename);
    }
}
