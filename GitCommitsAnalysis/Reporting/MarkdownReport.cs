using System;
using System.Linq;
using System.Text;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{

    public class MarkdownReport : BaseReport, IReport
    {
        public MarkdownReport(ISystemIO systemIO, string reportFilename, Options options) : base(systemIO, reportFilename, options)
        {
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating Markdown report...");
            StringBuilder sb = new StringBuilder();
            FileCommitsList = analysis.FileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            UserfileCommitsList = analysis.UserfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);

            sb.AppendLine($"# {Title}\n");
            sb.AppendLine($"Report created: {analysis.CreatedDate.ToString("yyyy-MM-dd")}\n");

            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            var numberOfAuthors = UserfileCommitsList.Select(ufc => ufc.Username).Distinct().Count();
            sb.AppendLine($"## Statistics\n");
            sb.AppendLine("| | |");
            sb.AppendLine("|---:|----:|");
            sb.AppendLine($"| First commit | {analysis.FirstCommitDate.ToString("yyyy-MM-dd")} |");
            sb.AppendLine($"| Latest commit | {analysis.LatestCommitDate.ToString("yyyy-MM-dd")} |");
            sb.AppendLine($"| Number of commits | {totalCommits} |");
            sb.AppendLine($"| Lines of code analysed | {analysis.LinesOfCodeAnalysed} |");
            sb.AppendLine($"| Number of authors | {numberOfAuthors} |");
            sb.AppendLine($"| Analysis time(milliseconds) | {analysis.AnalysisTime} |");
            sb.AppendLine();

            sb.AppendLine("## File changes for each subfolder");
            var folderCommitsList = analysis.FolderCommits.Values.OrderByDescending(fc => fc.FileChanges);
            foreach (var folder in folderCommitsList.Take(NumberOfFilesToList))
            {
                var folderName = string.Format("{0,50}", folder.FolderName);
                var changeCount = string.Format("{0,5}", analysis.FolderCommits[folder.FolderName].FileChanges);
                var percentage = string.Format("{0,5:#0.00}", ((double)analysis.FolderCommits[folder.FolderName].FileChanges / (double)totalCommits) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }
            sb.AppendLine($"{string.Format("{0,51}", "---------------")} {string.Format("{0,6}", "-----")} {string.Format("{0,7}", "------")}");
            var total = string.Format("{0,5}", totalCommits);
            sb.AppendLine($"{string.Format("{0,50}", "Total number of Commits analyzed")}: {total} ({string.Format("{0,5:##0.0}", 100)}%)\n");

            sb.AppendLine("---\n");

            foreach (var fileChange in FileCommitsList.Take(50))
            {
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
                var methodCount = fileChange.MethodCount > 0 ? fileChange.MethodCount.ToString() : "N/A";
                sb.AppendLine($"### {fileChange.Filename}\n");
                sb.AppendLine("| | |");
                sb.AppendLine("|---:|----:|");
                sb.AppendLine($"| Commits | {fileChange.CommitCount} |");
                sb.AppendLine($"| Lines of code | {linesOfCode} |");
                sb.AppendLine($"| Cyclomatic Complexity | {cyclomaticComplexity} |");
                sb.AppendLine($"| Method count | {methodCount} |");
                sb.AppendLine();

                sb.AppendLine("__Commits by user:__\n");
                sb.AppendLine($"| Name | Commits | Percentage |");
                sb.AppendLine($"|-----:|--------:|-----------:|");
                foreach (var userfileChange in UserfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
                {
                    var username = string.Format("{0,20}", userfileChange.Username);
                    var changeCount = string.Format("{0,3}", userfileChange.CommitCount);
                    var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.CommitCount / (double)fileChange.CommitCount) * 100);
                    sb.AppendLine($"| {username} | {changeCount} | {percentage}% |");
                }
                sb.AppendLine();
            }

            var tagsOrdered = analysis.Tags.AsEnumerable().OrderByDescending(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
            sb.AppendLine("## Tags\n");
            sb.AppendLine("| Name | Date |");
            sb.AppendLine("|---:|----:|");
            foreach (var kvp in tagsOrdered)
            {
                sb.AppendLine($"| {kvp.Value} | {kvp.Key.ToString("yyyy-MM-dd")} |");
            }
            sb.AppendLine();

            var fileTypesOrdered = analysis.FileTypes.AsEnumerable().OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key);
            sb.AppendLine("## Number of files of each type\n");
            sb.AppendLine("| | |");
            sb.AppendLine("|:---|----:|");
            foreach (var kvp in fileTypesOrdered)
            {
                sb.AppendLine($"| {kvp.Key} | {kvp.Value} |");
            }

            SystemIO.WriteAllText($"{ReportFilename}.md", sb.ToString());
        }
    }
}
