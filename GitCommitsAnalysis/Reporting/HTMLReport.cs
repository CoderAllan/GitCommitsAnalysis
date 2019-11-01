using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
    public class HTMLReport : IReport
    {
        private string reportFilename;
        private ISystemIO systemIO;

        private IOrderedEnumerable<FileStat> fileCommitsList;
        private IOrderedEnumerable<FileStat> userfileCommitsList;
        private IOrderedEnumerable<FileStat> folderCommitsList;
        private Dictionary<string, int> userNameKey = new Dictionary<string, int>();
        private Dictionary<string, FileStat> folderCommits;

        public HTMLReport(ISystemIO systemIO, string reportFilename)
        {
            this.systemIO = systemIO;
            this.reportFilename = reportFilename;
        }

        public void Generate(Dictionary<string, FileStat> fileCommits, Dictionary<string, FileStat> userfileCommits, Dictionary<string, FileStat> folderCommits)
        {
            Console.WriteLine("Generating HTML report...");
            if (fileCommits == null) throw new ArgumentException("Parameter fileCommits is null.", nameof(fileCommits));
            if (userfileCommits == null) throw new ArgumentException("Parameter userfileCommits is null.", nameof(userfileCommits));
            if (folderCommits == null) throw new ArgumentException("Parameter folderCommits is null.", nameof(folderCommits));
            this.fileCommitsList = fileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            this.userfileCommitsList = userfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
            var key = 1;
            foreach (var username in userfileCommits.Values.Select(fc => fc.Username).Distinct().OrderBy(un => un))
            {
                userNameKey.Add(username, key++);
            };
            this.folderCommits = folderCommits;
            this.folderCommitsList = folderCommits.Values.OrderByDescending(fc => fc.CommitCount);

            StringBuilder sb = new StringBuilder();
            AddHeader(sb);
            AddSectionCommitsForEachMonth(sb);
            var sectionCounter = 1;
            foreach (var fileChange in fileCommitsList.Take(50))
            {
                AddSectionCommitsForEachFile(sb, fileChange, sectionCounter++);
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
            sb.AppendLine("<h1 id = \"GitCommitsAnalysis\" >GitCommitsAnalysis</h1>");
        }

        private void AddPieChartJavascript(StringBuilder sb)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine("   var data = google.visualization.arrayToDataTable([['Folder', 'Commits'], ");
            foreach (var folder in folderCommitsList.Take(25))
            {
                sb.AppendLine($"      ['{folder.Filename}', {folder.CommitCount}],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { title: 'Commits for each subfolder' }; ");
            sb.AppendLine("   var chart = new google.visualization.PieChart(document.getElementById('piechart'));");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionCommitsForEachMonth(StringBuilder sb)
        {
            var totalCommits = fileCommitsList.Sum(fc => fc.CommitCount);
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("<h2>Commits for each subfolder</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine("<tr><th class=\"text-right\">Folder</th><th class=\"text-right\">Commits</th></tr>");
            foreach (var folder in folderCommitsList.Take(25))
            {
                var changeCount = string.Format("{0,5}", folderCommits[folder.Filename].CommitCount);
                var percentage = string.Format("{0,5:#0.0}", ((double)folderCommits[folder.Filename].CommitCount / (double)totalCommits) * 100);
                sb.AppendLine($"<tr><td class=\"text-right\">{folder.Filename}</td><td class=\"text-right\">{changeCount} ({percentage}%)</td></tr>");
            }
            var total = string.Format("{0,5}", totalCommits);
            sb.AppendLine($"<tr><td class=\"text-right\">Total number of Commits analyzed</td><td class=\"text-right\">{total} ({string.Format("{0,5:##0.0}", 100)}%)</td></tr>");
            sb.AppendLine("</table>\n");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            AddPieChartJavascript(sb);
            sb.AppendLine("<div id=\"piechart\" style=\"width: 900px; height: 500px; \"></div>");
            sb.AppendLine("</div></div>");
        }

        private void AddSectionCommitsForEachFile(StringBuilder sb, FileStat fileChange, int sectionCounter)
        {
            var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
            var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine($"<h3>{fileChange.Filename}</h3>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><td class=\"text-right\">Commits</td><td>{fileChange.CommitCount}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Lines of code</td><td>{linesOfCode}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Cyclomatic Complexity</td><td>{cyclomaticComplexity}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine($"<b>Commits by user</b>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><th>Name</th><th>Commits</th><th>Percentage</th></tr>");
            var commitDates = new List<ScatterPoint>();
            foreach (var userfileChange in userfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
            {
                var username = string.Format("{0,20}", userfileChange.Username);
                var changeCount = string.Format("{0,3}", userfileChange.CommitCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.CommitCount / (double)fileChange.CommitCount) * 100);
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
