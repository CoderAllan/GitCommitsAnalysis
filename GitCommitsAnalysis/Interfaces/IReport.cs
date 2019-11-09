using GitCommitsAnalysis.Model;
using System;
using System.Collections.Generic;

namespace GitCommitsAnalysis.Interfaces
{
    public interface IReport
    {
        void Generate(
            Dictionary<string, FileStat> fileCommits,
            Dictionary<string, FileStat> userfileCommits,
            Dictionary<string, FileStat> folderCommits,
            Dictionary<DateTime, int> commitsEachDay
        );
    }
}
