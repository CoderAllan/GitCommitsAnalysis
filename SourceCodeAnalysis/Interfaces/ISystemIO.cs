﻿namespace SourceCodeAnalysis.Interfaces
{
    public interface ISystemIO
    {
        string ReadFileContent(string filename);
        void WriteAllText(string filename, string contents);
        bool FileExists(string filename);
        string GetPathWitoutExtension(string filename);
    }
}
