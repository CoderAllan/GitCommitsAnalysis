using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommitsAnalysis.Reporting
{
    public class BaseReport
    {
        protected string ReportFilename { get; set; }
        protected int NumberOfFilesToList { get; set; }
        protected ISystemIO SystemIO { get; set; }
        protected IOrderedEnumerable<FileStat> FileCommitsList { get; set; }
        protected IOrderedEnumerable<FileStat> UserfileCommitsList { get; set; }
        protected IOrderedEnumerable<FileStat> FolderCommitsList { get; set; }
        protected Dictionary<string, int> UserNameKey { get; } = new Dictionary<string, int>();
        protected Dictionary<string, FileStat> FolderCommits { get; set; } = new Dictionary<string, FileStat>();
        protected BaseReport(ISystemIO systemIO, string reportFilename, int numberOfFilesToList)
        {
            this.SystemIO = systemIO;
            this.ReportFilename = reportFilename;
            this.NumberOfFilesToList = numberOfFilesToList;
        }
    }
}
