using System;
using System.Linq;
using System.Text;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
    public class TextFileReport : BaseReport, IReport
    {
        public TextFileReport(ISystemIO systemIO, string reportFilename, Options options) : base(systemIO, reportFilename, options)
        {
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating Text file report...");
            StringBuilder sb = new StringBuilder();
            FileCommitsList = analysis.FileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            UserfileCommitsList = analysis.UserfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);

            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            var numberOfAuthors = UserfileCommitsList.Select(ufc => ufc.Username).Distinct().Count();
            sb.AppendLine($"{Title}\n");
            sb.AppendLine($"Report created: {analysis.CreatedDate.ToString("yyyy-MM-dd")}");
            sb.AppendLine($"First commit: {analysis.FirstCommitDate.ToString("yyyy-MM-dd")}");
            sb.AppendLine($"Latest commit: {analysis.LatestCommitDate.ToString("yyyy-MM-dd")}");
            sb.AppendLine($"Number of commits: {totalCommits}");
            sb.AppendLine($"Lines of code analyzed: {analysis.LinesOfCodeanalyzed}");
            sb.AppendLine($"Number of authors: {numberOfAuthors}");
            sb.AppendLine($"Analysis time(milliseconds): {analysis.AnalysisTime}");
            sb.AppendLine();

            var folderCommitsList = analysis.FolderCommits.Values.OrderByDescending(fc => fc.FileChanges);
            foreach (var folder in folderCommitsList.Take(NumberOfFilesToList))
            {
                var folderName = string.Format("{0,50}", folder.FolderName);
                var changeCount = string.Format("{0,5}", analysis.FolderCommits[folder.FolderName].FileChanges);
                var percentage = string.Format("{0,5:#0.00}", ((double)analysis.FolderCommits[folder.FolderName].FileChanges / (double)totalCommits) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }

            foreach (var fileChange in FileCommitsList.Take(50))
            {
                sb.AppendLine("");
                string fileInfo = "File has been deleted";
                if (fileChange.FileExists) {
                    var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                    var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
                    var methodCount = fileChange.MethodCount > 0 ? fileChange.MethodCount.ToString() : "N/A";
                    fileInfo = $"Lines of code: {linesOfCode} - Cyclomatic Complexity: {cyclomaticComplexity} - Method count: {methodCount}";
                }
                sb.AppendLine($"{fileChange.Filename}: {fileChange.CommitCount} - Latest commit: {fileChange.LatestCommit.ToString("yyyy-MM-dd")} - {fileInfo}\n");
                foreach (var userfileChange in UserfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
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
