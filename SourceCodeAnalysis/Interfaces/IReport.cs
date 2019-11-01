using GitCommitsAnalysis.Model;
using System.Collections.Generic;

namespace GitCommitsAnalysis.Interfaces
{
    public interface IReport
    {
        void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges);
    }
}
