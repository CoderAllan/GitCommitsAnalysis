using System;
using System.Collections.Generic;

namespace GitCommitsAnalysis.Model
{
    public class Analysis
    {
        public DateTime CreatedDate { get; } = DateTime.UtcNow;
        public long AnalysisTime { get; set; }
        public Dictionary<DateTime, int> CommitsEachDay { get; } = new Dictionary<DateTime, int>();
        public Dictionary<DateTime, int> LinesOfCodeAddedEachDay { get; } = new Dictionary<DateTime, int>();
        public Dictionary<DateTime, int> LinesOfCodeDeletedEachDay { get; } = new Dictionary<DateTime, int>();
        public Dictionary<string, FileStat> FileCommits { get; } = new Dictionary<string, FileStat>();
        public Dictionary<string, FileStat> FolderCommits { get; } = new Dictionary<string, FileStat>();
        public Dictionary<string, FileStat> UserfileCommits { get; } = new Dictionary<string, FileStat>();
    }
}
