using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Model;

namespace SourceCodeAnalysis.Reporting
{
    public class HTMLReport : IReport
    {
        private string reportFilename;
        private ISystemIO systemIO;

        private IOrderedEnumerable<FileStat> fileChangesList;
        private IOrderedEnumerable<FileStat> userfileChangesList;
        private IOrderedEnumerable<FileStat> folderChangesList;
        private Dictionary<string, int> userNameKey = new Dictionary<string, int>();
        private Dictionary<string, FileStat> folderChanges;

        public HTMLReport(ISystemIO systemIO, string reportFilename)
        {
            this.systemIO = systemIO;
            this.reportFilename = reportFilename;
        }

        public void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges)
        {
            Console.WriteLine("Generating HTML report...");
            if (fileChanges == null) throw new ArgumentException("Parameter fileChanges is null.", nameof(fileChanges));
            if (userfileChanges == null) throw new ArgumentException("Parameter userfileChanges is null.", nameof(userfileChanges));
            if (folderChanges == null) throw new ArgumentException("Parameter folderChanges is null.", nameof(folderChanges));
            this.fileChangesList = fileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename);
            this.userfileChangesList = userfileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
            var key = 1;
            foreach (var username in userfileChanges.Values.Select(fc => fc.Username).Distinct().OrderBy(un => un))
            {
                userNameKey.Add(username, key++);
            };
            this.folderChanges = folderChanges;
            this.folderChangesList = folderChanges.Values.OrderByDescending(fc => fc.ChangeCount);

            StringBuilder sb = new StringBuilder();
            AddHeader(sb);
            AddSectionChangesForEachMonth(sb);
            var sectionCounter = 1;
            foreach (var fileChange in fileChangesList.Take(50))
            {
                AddSectionChangesForEachFile(sb, fileChange, sectionCounter++);
            }
            sb.AppendLine("</div>");

            systemIO.WriteAllText($"{reportFilename}.html", sb.ToString());
        }

        private void AddHeader(StringBuilder sb)
        {
            sb.AppendLine("<html><head>");
            sb.AppendLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css\" integrity=\"sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu\" crossorigin=\"anonymous\">");
            sb.AppendLine("<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>");
            sb.AppendLine("<style>");
            sb.AppendLine("body {");
            sb.AppendLine("   color: black;");
            sb.AppendLine("}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine("<h1 id = \"sourcecodeanalysis\" >SourceCodeAnalysis</h1>");
        }

        private void AddPieChartJavascript(StringBuilder sb)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine("   var data = google.visualization.arrayToDataTable([['Folder', 'Changes'], ");
            foreach (var folder in folderChangesList.Take(25))
            {
                sb.AppendLine($"      ['{folder.Filename}', {folder.ChangeCount}],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { title: 'Changes for each subfolder' }; ");
            sb.AppendLine("   var chart = new google.visualization.PieChart(document.getElementById('piechart'));");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionChangesForEachMonth(StringBuilder sb)
        {
            var totalChanges = fileChangesList.Sum(fc => fc.ChangeCount);
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("<h2>Changes for each subfolder</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine("<tr><th class=\"text-right\">Folder</th><th class=\"text-right\">Changes</th></tr>");
            foreach (var folder in folderChangesList.Take(25))
            {
                var changeCount = string.Format("{0,5}", folderChanges[folder.Filename].ChangeCount);
                var percentage = string.Format("{0,5:#0.0}", ((double)folderChanges[folder.Filename].ChangeCount / (double)totalChanges) * 100);
                sb.AppendLine($"<tr><td class=\"text-right\">{folder.Filename}</td><td class=\"text-right\">{changeCount} ({percentage}%)</td></tr>");
            }
            var total = string.Format("{0,5}", totalChanges);
            sb.AppendLine($"<tr><td class=\"text-right\">Total number of changes analyzed</td><td class=\"text-right\">{total} ({string.Format("{0,5:##0.0}", 100)}%)</td></tr>");
            sb.AppendLine("</table>\n");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            AddPieChartJavascript(sb);
            sb.AppendLine("<div id=\"piechart\" style=\"width: 900px; height: 500px; \"></div>");
            sb.AppendLine("</div></div>");
        }

        private void AddSectionChangesForEachFile(StringBuilder sb, FileStat fileChange, int sectionCounter)
        {
            var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
            var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine($"<h3>{fileChange.Filename}</h3>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><td class=\"text-right\">Changes</td><td>{fileChange.ChangeCount}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Lines of code</td><td>{linesOfCode}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Cyclomatic Complexity</td><td>{cyclomaticComplexity}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine($"<b>Changes by user</b>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><th>Name</th><th>Changes</th><th>Percentage</th></tr>");
            var commitDates = new List<ScatterPoint>();
            foreach (var userfileChange in userfileChangesList.Where(ufc => ufc.Filename == fileChange.Filename))
            {
                var username = string.Format("{0,20}", userfileChange.Username);
                var changeCount = string.Format("{0,3}", userfileChange.ChangeCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.ChangeCount / (double)fileChange.ChangeCount) * 100);
                sb.AppendLine($"<tr><td class=\"text-right\">{username}</td><td>{changeCount}</td><td>{percentage}%</td></tr>");
                GenerateScatterPlotData(commitDates, userfileChange);
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class=\"col-md-6\">");
            AddScatterplotJavascript(sb, commitDates, sectionCounter);
            sb.AppendLine($"    <div id=\"scatterChart{sectionCounter}\" style=\"width: 900px; height: 300px; \"></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
        }

        private void GenerateScatterPlotData(List<ScatterPoint> commitDates, FileStat fileStat)
        {
            foreach (var commitDate in fileStat.CommitDates)
            {
                var date = $"new Date('{commitDate.ToString("yyyy-MM-dd")}')";
                var userId = userNameKey[fileStat.Username];
                if (!commitDates.Any(cd => cd.Date == date && cd.UserId == userId)) // Only add a point for each user for each date
                {
                    commitDates.Add(new ScatterPoint
                    {
                        Date = date,
                        UserId = userId,
                        ToolTip = $"'{commitDate.ToString("yyyy-MM-dd")}, {fileStat.Username}'"
                    });
                }
            }
        }

        private static void AddScatterplotJavascript(StringBuilder sb, List<ScatterPoint> commitDates, int sectionCounter)
        {
            var commitDatesString = string.Join(",", commitDates.Select(sp => sp.ToString()).OrderBy(sp => sp));

            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart'] });");
            sb.AppendLine($"google.charts.setOnLoadCallback(drawChart{sectionCounter});");
            sb.AppendLine($"function drawChart{sectionCounter}() {{");
            sb.AppendLine($"var data{sectionCounter} = new google.visualization.DataTable();");
            sb.AppendLine($"data{sectionCounter}.addColumn('date', 'Date');");
            sb.AppendLine($"data{sectionCounter}.addColumn('number', 'UserId');");
            sb.AppendLine($"data{sectionCounter}.addColumn({{ type: 'string', role: 'tooltip'}});");
            sb.AppendLine($"data{sectionCounter}.addRows([");
            sb.AppendLine(commitDatesString);
            sb.AppendLine("]);");
            sb.AppendLine("var options = { title: 'User commits for each date', legend: 'none' };");
            sb.AppendLine($"var chart = new google.visualization.ScatterChart(document.getElementById('scatterChart{sectionCounter}'));");
            sb.AppendLine($"chart.draw(data{sectionCounter}, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

    }
}
