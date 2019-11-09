using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
    public class TextFileReport : BaseReport, IReport
    {
        public TextFileReport(ISystemIO systemIO, string reportFilename, string title, int numberOfFilesToList) : base(systemIO, reportFilename, title, numberOfFilesToList)
        {
        }

        public void Generate(
            Dictionary<string, FileStat> fileCommits,
            Dictionary<string, FileStat> userfileCommits,
            Dictionary<string, FileStat> folderCommits,
            Dictionary<DateTime, int> commitsEachDay,
            Dictionary<DateTime, int> linesOfCodeAddedEachDay,
            Dictionary<DateTime, int> linesOfCodeDeletedEachDay
            )
        {
            Console.WriteLine("Generating Textfile report...");
            StringBuilder sb = new StringBuilder();
            var fileCommitsList = fileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            var userfileCommitsList = userfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);

            var totalCommits = fileCommitsList.Sum(fc => fc.CommitCount);
            sb.AppendLine($"{Title}\n");
            sb.AppendLine($"Total number of Commits analyzed: {totalCommits}");

            var folderCommitsList = folderCommits.Values.OrderByDescending(fc => fc.CommitCount);
            foreach (var folder in folderCommitsList.Take(NumberOfFilesToList))
            {
                var folderName = string.Format("{0,50}", folder.Filename);
                var changeCount = string.Format("{0,5}", folderCommits[folder.Filename].CommitCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)folderCommits[folder.Filename].CommitCount / (double)totalCommits) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }

            foreach (var fileChange in fileCommitsList.Take(50))
            {
                sb.AppendLine("");
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
                sb.AppendLine($"{fileChange.Filename}: {fileChange.CommitCount} - Lines of code: {linesOfCode} - Cyclomatic Complexity: {cyclomaticComplexity}\n");
                foreach (var userfileChange in userfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
                {
                    var username = string.Format("{0,20}", userfileChange.Username);
                    var changeCount = string.Format("{0,3}", userfileChange.CommitCount);
                    var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.CommitCount / (double)fileChange.CommitCount) * 100);
                    sb.AppendLine($"    {username}: {changeCount} ({percentage}%)");
                }
            }

            SystemIO.WriteAllText($"{ReportFilename}.txt", sb.ToString());
        }
    }
}
