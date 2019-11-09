using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
   
    public class MarkdownReport : BaseReport, IReport
    {
        public MarkdownReport(ISystemIO systemIO, string reportFilename, int numberOfFilesToList) : base(systemIO, reportFilename, numberOfFilesToList)
        {
        }

        public void Generate(Dictionary<string, FileStat> fileCommits, Dictionary<string, FileStat> userfileCommits, Dictionary<string, FileStat> folderCommits, Dictionary<DateTime, int> commitsEachDay)
        {
            Console.WriteLine("Generating Markdown report...");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# GitCommitsAnalysis\n");
            var fileCommitsList = fileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            var userfileCommitsList = userfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);


            var totalCommits = fileCommitsList.Sum(fc => fc.CommitCount);
            sb.AppendLine("## Commits for each subfolder");
            var folderCommitsList = folderCommits.Values.OrderByDescending(fc => fc.CommitCount);
            foreach (var folder in folderCommitsList.Take(NumberOfFilesToList))
            {
                var folderName = string.Format("{0,50}", folder.Filename);
                var changeCount = string.Format("{0,5}", folderCommits[folder.Filename].CommitCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)folderCommits[folder.Filename].CommitCount / (double)totalCommits) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }
            sb.AppendLine($"{string.Format("{0,51}", "---------------")} {string.Format("{0,6}", "-----")} {string.Format("{0,7}", "------")}");
            var total = string.Format("{0,5}", totalCommits);
            sb.AppendLine($"{string.Format("{0,50}", "Total number of Commits analyzed")}: {total} ({string.Format("{0,5:##0.0}", 100)}%)\n");

            sb.AppendLine("---\n");

            foreach (var fileChange in fileCommitsList.Take(50))
            {
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
                sb.AppendLine($"### {fileChange.Filename}\n");
                sb.AppendLine("| | |");
                sb.AppendLine("|---:|----:|");
                sb.AppendLine($"| Commits | {fileChange.CommitCount} |");
                sb.AppendLine($"| Lines of code | {linesOfCode} |");
                sb.AppendLine($"| Cyclomatic Complexity | {cyclomaticComplexity} |");
                sb.AppendLine();

                sb.AppendLine("__Commits by user:__\n");
                sb.AppendLine($"| Name | Commits | Percentage |");
                sb.AppendLine($"|-----:|--------:|-----------:|");
                foreach (var userfileChange in userfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
                {
                    var username = string.Format("{0,20}", userfileChange.Username);
                    var changeCount = string.Format("{0,3}", userfileChange.CommitCount);
                    var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.CommitCount / (double)fileChange.CommitCount) * 100);
                    sb.AppendLine($"| {username} | {changeCount} | {percentage}% |");
                }
                sb.AppendLine();
            }
            SystemIO.WriteAllText($"{ReportFilename}.md", sb.ToString());
        }
    }
}
