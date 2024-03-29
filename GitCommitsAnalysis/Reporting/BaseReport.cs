﻿using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;
using System.Collections.Generic;
using System.Linq;

namespace GitCommitsAnalysis.Reporting
{
    public class BaseReport
    {
        protected string ReportFilename { get; set; }
        protected string Title { get; set; }
        protected int NumberOfFilesToList { get; set; }
        protected ISystemIO SystemIO { get; set; }
        protected IOrderedEnumerable<FileStat> FileCommitsList { get; set; }
        protected IOrderedEnumerable<FileStat> UserfileCommitsList { get; set; }
        protected IOrderedEnumerable<FolderStat> FolderCommitsList { get; set; }
        protected Dictionary<string, int> UserNameKey { get; } = new Dictionary<string, int>();
        protected Dictionary<string, FolderStat> FolderCommits { get; set; }
        protected BaseReport(ISystemIO systemIO, string reportFilename, Options options)
        {
            this.SystemIO = systemIO;
            this.ReportFilename = reportFilename;
            this.Title = options.Title;
            this.NumberOfFilesToList = options.NumberOfFilesInList;
        }
    }
}
