using SourceCodeAnalysis.Model;
using System.Collections.Generic;

namespace SourceCodeAnalysis.Interfaces
{
    public interface IReport
    {
        void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges);
    }
}
