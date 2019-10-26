using System.Collections.Generic;
using System.Linq;
using System.IO;
using SourceCodeAnalysis.Model;
using SourceCodeAnalysis.Interfaces;
using System.Text;

namespace SourceCodeAnalysis.Reporting
{
    public class TextFileReport : IReport
    {
        private string reportFilename;
        public TextFileReport(string reportFilename)
        {
            this.reportFilename = reportFilename;
        }

        public void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges)
        {
            StringBuilder sb = new StringBuilder();
            var fileChangesList = fileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename);
            var userfileChangesList = userfileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
            var totalChanges = fileChangesList.Sum(fc => fc.ChangeCount);
            sb.AppendLine($"Total number of changes analyzed: {totalChanges}");
            foreach (var fileChange in fileChangesList.Take(50))
            {
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                sb.AppendLine($"{fileChange.Filename}: {fileChange.ChangeCount} - Lines of code: {linesOfCode}");
                foreach (var userfileChange in userfileChangesList.Where(ufc => ufc.Filename == fileChange.Filename))
                {
                    var username = string.Format("{0,20}", userfileChange.Username);
                    var changeCount = string.Format("{0,3}", userfileChange.ChangeCount);
                    var percentage = string.Format("{0,5:##.00}", ((double)userfileChange.ChangeCount / (double)fileChange.ChangeCount) * 100);
                    sb.AppendLine($"    {username}: {changeCount} ({percentage}%)");
                }
            }
            sb.AppendLine("-------");
            var folderChangesList = folderChanges.Values.OrderByDescending(fc => fc.ChangeCount);
            foreach (var folder in folderChangesList.Take(25))
            {
                var folderName = string.Format("{0,50}", folder.Filename);
                var changeCount = string.Format("{0,5}", folderChanges[folder.Filename].ChangeCount);
                var percentage = string.Format("{0,5:##.00}", ((double)folderChanges[folder.Filename].ChangeCount / (double)totalChanges) * 100);
                sb.AppendLine($"{folderName}: {changeCount} ({percentage}%)");
            }
            File.WriteAllText(reportFilename, sb.ToString());
        }
    }
}
