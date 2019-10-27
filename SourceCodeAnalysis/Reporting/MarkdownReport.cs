using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Model;

namespace SourceCodeAnalysis.Reporting
{
   
    public class MarkdownReport : IReport
    {
        private string reportFilename;
        private ISystemIO systemIO;
        public MarkdownReport(ISystemIO systemIO, string reportFilename)
        {
            this.systemIO = systemIO;
            this.reportFilename = reportFilename;
        }

        public void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges)
        {
            Console.WriteLine("Generating Markdown report...");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# SourceCodeAnalysis\n");
            var fileChangesList = fileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename);
            var userfileChangesList = userfileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);


            var totalChanges = fileChangesList.Sum(fc => fc.ChangeCount);
            sb.AppendLine("## Changes for each subfolder");
            var folderChangesList = folderChanges.Values.OrderByDescending(fc => fc.ChangeCount);
            foreach (var folder in folderChangesList.Take(25))
            {
                var folderName = string.Format("{0,50}", folder.Filename);
                var changeCount = string.Format("{0,5}", folderChanges[folder.Filename].ChangeCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)folderChanges[folder.Filename].ChangeCount / (double)totalChanges) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }
            sb.AppendLine($"{string.Format("{0,51}", "---------------")} {string.Format("{0,6}", "-----")} {string.Format("{0,7}", "------")}");
            var total = string.Format("{0,5}", totalChanges);
            sb.AppendLine($"{string.Format("{0,50}", "Total number of changes analyzed")}: {total} ({string.Format("{0,5:##0.0}", 100)}%)\n");

            sb.AppendLine("---\n");

            foreach (var fileChange in fileChangesList.Take(50))
            {
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                sb.AppendLine($"### {fileChange.Filename}\n");
                sb.AppendLine("| | |");
                sb.AppendLine("|---:|----:|");
                sb.AppendLine($"| Changes | {fileChange.ChangeCount} |");
                sb.AppendLine($"| Lines of code | {linesOfCode} |");
                sb.AppendLine($"| Cyclomatic Complexity | {cyclomaticComplexity} |");
                sb.AppendLine();

                sb.AppendLine("__Changes by user:__\n");
                sb.AppendLine($"| Name | Changes | Percentage |");
                sb.AppendLine($"|-----:|--------:|-----------:|");
                foreach (var userfileChange in userfileChangesList.Where(ufc => ufc.Filename == fileChange.Filename))
                {
                    var username = string.Format("{0,20}", userfileChange.Username);
                    var changeCount = string.Format("{0,3}", userfileChange.ChangeCount);
                    var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.ChangeCount / (double)fileChange.ChangeCount) * 100);
                    sb.AppendLine($"| {username} | {changeCount} | {percentage}% |");
                }
                sb.AppendLine();
            }
            systemIO.WriteAllText(reportFilename, sb.ToString());
        }
    }
}
