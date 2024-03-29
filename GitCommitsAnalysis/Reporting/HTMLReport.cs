﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
    public class HTMLReport : BaseReport, IReport
    {
        public HTMLReport(ISystemIO systemIO, string reportFilename, Options options) : base(systemIO, reportFilename, options)
        {
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating HTML report...");
            if (analysis == null) throw new ArgumentException("Parameter analysis is null.", nameof(analysis));
            this.FileCommitsList = analysis.FileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            this.UserfileCommitsList = analysis.UserfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
            var key = 1;
            foreach (var username in analysis.UserfileCommits.Values.Select(fc => fc.Username).Distinct().OrderBy(un => un))
            {
                UserNameKey.Add(username, key++);
            };
            this.FolderCommits = analysis.FolderCommits;
            this.FolderCommitsList = analysis.FolderCommits.Values.OrderByDescending(fc => fc.FileChanges);

            StringBuilder sb = new StringBuilder();
            AddHeader(sb, analysis);
            AddNavTabs(sb);
            sb.AppendLine("<div class=\"tab-content\">");
            sb.AppendLine("<div role=\"tabpanel\" class=\"tab-pane active\" id=\"commitsForEachSubfolder\">");
            AddSectionCommitsForEachFolder(sb);
            sb.Append($"<h2 id=\"mostChangedFiles\">Top {NumberOfFilesToList} most changed files</h2>");
            var sectionCounter = 1;
            foreach (var fileChange in FileCommitsList.Take(NumberOfFilesToList))
            {
                AddSectionCommitsForEachFile(sb, fileChange, sectionCounter++);
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<div role=\"tabpanel\" class=\"tab-pane\" id=\"projectStatistics\">");
            AddSectionProjectStatistics(sb, analysis);
            AddSectionCommitsForEachDay(sb, analysis.CommitsEachDay);
            AddSectionLinesChangedEachDay(sb, analysis.LinesOfCodeAddedEachDay, analysis.LinesOfCodeDeletedEachDay);
            AddSectionCodeAge(sb, analysis.CodeAge);
            if (analysis.Tags.Any())
            {
                AddSectionTags(sb, analysis.Tags);
            }
            if (analysis.Branches.Any())
            {
                AddSectionBranches(sb, analysis.Branches);
            }
            AddSectionFileTypes(sb, analysis.FileTypes);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            AddFooter(sb);

            SystemIO.WriteAllText($"{ReportFilename}.html", sb.ToString());
        }

        private void AddHeader(StringBuilder sb, Analysis analysis)
        {
            sb.AppendLine("<html><head>");
            sb.AppendLine($"<title>{Title}</title>");
            sb.AppendLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css\" integrity=\"sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu\" crossorigin=\"anonymous\" type=\"text/css\">");
            sb.AppendLine("<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.11.2/css/all.min.css\" type=\"text/css\">");
            sb.AppendLine("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js\"></script>");
            sb.AppendLine("<script src=\"https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js\" integrity=\"sha384-aJ21OjlMXNL5UyIl/XNwTMqvzeRMZH2w8c5cRVpzpU8Y5bApTppSuUkhZXN0VxHd\" crossorigin=\"anonymous\"></script>");
            sb.AppendLine("<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>");
            sb.AppendLine("<style>");
            sb.AppendLine("body {");
            sb.AppendLine("   color: black;");
            sb.AppendLine("}");
            sb.AppendLine("ul, #myUL {");
            sb.AppendLine("  list-style-type: none;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("#myUL {");
            sb.AppendLine("  margin: 0;");
            sb.AppendLine("  padding: 0;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".treeViewCaret {");
            sb.AppendLine("  cursor: pointer;");
            sb.AppendLine("  user-select: none; /* Prevent text selection */");
            sb.AppendLine("  white-space: nowrap;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".treeViewCaret::before {");
            sb.AppendLine("  content: \"\\25B6\";");
            sb.AppendLine("  color: black;");
            sb.AppendLine("  display: inline-block;");
            sb.AppendLine("  margin-right: 6px;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".caret-down::before {");
            sb.AppendLine("  transform: rotate(90deg);");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".nested {");
            sb.AppendLine("  display: none;");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine(".active {");
            sb.AppendLine("  display: contents;");
            sb.AppendLine("}");
            sb.AppendLine(".iw {");
            sb.AppendLine("  padding-right: 5px;");
            sb.AppendLine("}");
            sb.AppendLine(".pl20 {");
            sb.AppendLine("  padding-left: 20px;");
            sb.AppendLine("}");
            sb.AppendLine(".pl40 {");
            sb.AppendLine("  padding-left: 40px;");
            sb.AppendLine("}");
            sb.AppendLine(".spinner {");
            sb.AppendLine("  height: 48px;");
            sb.AppendLine("  width: 48px;");
            sb.AppendLine("  border: 5px solid rgba(150, 150, 150, 0.2);");
            sb.AppendLine("  border-radius: 50%;");
            sb.AppendLine("  border-top-color: rgb(150, 150, 150);");
            sb.AppendLine("  animation: rotate 1s 0s infinite linear normal;");
            sb.AppendLine("}");
            sb.AppendLine("@keyframes rotate {");
            sb.AppendLine("  0%   { transform: rotate(0);      }");
            sb.AppendLine("  100% { transform: rotate(360deg); }");
            sb.AppendLine("}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h1 id=\"gitCommitsAnalysis\">{Title}</h1>");
            sb.AppendLine($"<div class=\"text-right\">Report created: {analysis.CreatedDate.ToString("yyyy-MM-dd")}</div>");
        }

        private void AddFooter(StringBuilder sb)
        {
            sb.AppendLine("<div class=\"row\">This report was generated by <a href=\"https://github.com/CoderAllan/GitCommitsAnalysis\">GitCommitsAnalysis</a>.</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
        }

        private void AddNavTabs(StringBuilder sb)
        {
            sb.AppendLine("<script type=\"javascript\">");
            sb.AppendLine("$('#tabs a[href=\"#commitsForEachSubfolder\"]').click(function (e) { e.preventDefault() $(this).tab('show')})");
            sb.AppendLine("$('#tabs a[href=\"#activityEachDay\"]').click(function (e) { e.preventDefault() $(this).tab('show')})");
            sb.AppendLine("</script>");
            sb.AppendLine("<ul class=\"nav nav-tabs\" role=\"tablist\" id=\"tabs\">");
            sb.AppendLine("<li role=\"presentation\" class=\"active\"><a href=\"#commitsForEachSubfolder\" aria-controls=\"home\" role=\"tab\" data-toggle=\"tab\">Commits for each sub-folder</a></li>");
            sb.AppendLine("<li role=\"presentation\"><a href=\"#projectStatistics\" aria-controls=\"projectStatistics\" role=\"tab\" data-toggle=\"tab\">Statistics</a></li>");
            sb.AppendLine("</ul>");
        }

        private void AddPieChartJavascript(StringBuilder sb)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine("   var data = google.visualization.arrayToDataTable([['Folder', 'Commits'], ");
            foreach (var folder in FolderCommitsList.Take(25))
            {
                sb.AppendLine($"      ['{folder.FolderName}', {folder.FileChanges}],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { title: 'Commits for each sub-folder' }; ");
            sb.AppendLine("   var element = document.getElementById('piechart');");
            sb.AppendLine("   element.classList.remove(\"spinner\");");
            sb.AppendLine("   element.style = \"width: 900px; height: 500px;\"");
            sb.AppendLine("   var chart = new google.visualization.PieChart(element);");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionCommitsForEachFolder(StringBuilder sb)
        {
            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("<h2>Commits for each sub-folder</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine("<tr><th class=\"text-right\">Folder</th><th class=\"text-right\">File changes</th><th></th></tr>");
            int folderCounter = 1;
            foreach (var folder in FolderCommitsList.Take(25))
            {
                var changeCount = string.Format("{0,5}", FolderCommits[folder.FolderName].FileChanges);
                var percentage = string.Format("{0,5:#0.0}", ((double)FolderCommits[folder.FolderName].FileChanges / (double)totalCommits) * 100);
                var expand = folder.Children.Keys.Count > 0 ? $"<span onclick=\"expandFolder({folderCounter})\" id=\"folderSpanExpand{folderCounter}\" class=\"treeViewCaret\">Expand</span>" : "";
                sb.AppendLine($"<tr><td class=\"text-right\">{WebUtility.HtmlEncode(folder.FolderName)}</td><td class=\"text-right text-nowrap\">{changeCount} ({percentage}%)</td><td>{expand}</td></tr>");
                if (folder.Children.Keys.Count > 0)
                {
                    sb.AppendLine($"<tr id=\"folderTrExpand{folderCounter}\" class=\"nested\"><td colspan=\"3\" style=\"overflow: scroll\">");
                    sb.AppendLine($"<ul id=\"folderUlExpand{folderCounter}\" class=\"nested\">");
                    AddSectionCommitsForEachFolderChildren(sb, folder, 0);
                    sb.AppendLine("</ul>");
                    sb.AppendLine("</td></tr>");
                }
                folderCounter++;
            }
            var total = string.Format("{0,5}", totalCommits);
            sb.AppendLine($"<tr><td class=\"text-right\">Total number of Commits analyzed</td><td class=\"text-right\">{total} ({string.Format("{0,5:##0.0}", 100)}%)</td></td></tr>");
            sb.AppendLine("</table>\n");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            AddPieChartJavascript(sb);
            sb.AppendLine("<div id=\"piechart\" style=\"width: 48px; height: 48px;\" class=\"spinner\"></div>");
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("var toggler = document.getElementsByClassName(\"treeViewCaret\");");
            sb.AppendLine("var i;");
            sb.AppendLine("for (i = 0; i < toggler.length; i++) {");
            sb.AppendLine("  toggler[i].addEventListener(\"click\", function() {");
            sb.AppendLine("    var children = this.parentElement.querySelector(\".nested\");");
            sb.AppendLine("    if(children) {");
            sb.AppendLine("       children.classList.toggle(\"active\");");
            sb.AppendLine("       this.classList.toggle(\"caret-down\");");
            sb.AppendLine("    }");
            sb.AppendLine("  });");
            sb.AppendLine("}");
            sb.AppendLine("function expandFolder(folderId) {");
            sb.AppendLine("  var folderExpander = document.getElementById('folderTrExpand' + folderId);");
            sb.AppendLine("  folderExpander.classList.toggle('active');");
            sb.AppendLine("  folderExpander = document.getElementById('folderUlExpand' + folderId);");
            sb.AppendLine("  folderExpander.classList.toggle('active');");
            sb.AppendLine("  folderExpander = document.getElementById('folderSpanExpand' + folderId)");
            sb.AppendLine("  folderExpander.classList.toggle('caret-down');");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            sb.AppendLine("</div></div>");
        }

        private void AddSectionCommitsForEachFolderChildren(StringBuilder sb, FolderStat parentFolder, int indent)
        {
            if (parentFolder.Children.Keys.Count > 0)
            {
                foreach (var folder in parentFolder.Children.Values.OrderByDescending(fs => fs.FileChanges))
                {
                    var changeCount = string.Format("{0,5}", parentFolder.Children[folder.FolderName].FileChanges);
                    var icon = FileIcon(folder.FolderName);
                    var folderName = folder.Children.Keys.Count > 0 ? $"<span class=\"treeViewCaret\">{WebUtility.HtmlEncode(folder.FolderName)}</span>" : $"<i class=\"{icon} iw\"></i>{WebUtility.HtmlEncode(folder.FolderName)}";
                    var padding = folder.Children.Keys.Count <= 0 ? "pl40" : "pl20";
                    sb.AppendLine($"<li class=\"text-nowrap {padding}\">{folderName}: {changeCount}");
                    if (folder.Children.Keys.Count > 0)
                    {
                        sb.AppendLine($"<ul class=\"nested\">");
                        AddSectionCommitsForEachFolderChildren(sb, folder, indent + 1);
                        sb.AppendLine("</ul>");
                    }
                    sb.AppendLine("</li>");
                }
            }
        }

        private string FileIcon(string filename)
        {
            var extension = SystemIO.GetExtension(filename);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            var cssClass = "";
            switch (extension)
            {
                case "html":
                case "cshtml":
                case "ts":
                case "cs":
                case "ps1":
                case "bat":
                case "cmd":
                case "sh":
                case "json":
                case "xml":
                case "css":
                case "scss":
                    cssClass += "far fa-file-code";
                    break;
                case "js":
                    cssClass += "fab fa-js";
                    break;
                case "xls":
                case "xlsx":
                    cssClass += "far fa-file-excel";
                    break;
                case "csv":
                    cssClass += "far fa-file-csv";
                    break;
                case "doc":
                case "docx":
                    cssClass += "far fa-file-word";
                    break;
                case "pdf":
                    cssClass += "far fa-file-pdf";
                    break;
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "svg":
                case "tiff":
                case "tif":
                case "bmp":
                case "ico":
                    cssClass += "far fa-file-image";
                    break;
                case "txt":
                    cssClass += "far fa-file-alt";
                    break;
                case "md":
                    cssClass += "fab fa-markdown";
                    break;
                case "zip":
                case "tgz":
                case "tar":
                case "rar":
                    cssClass += "far fa-file-archive";
                    break;
                case "eot":
                case "otf":
                case "ttf":
                case "woff":
                case "woff2":
                    cssClass += "fas fa-font";
                    break;
                default:
                    cssClass += "far fa-file";
                    break;
            }
            return cssClass;
        }

        private void AddSectionProjectStatistics(StringBuilder sb, Analysis analysis)
        {
            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            var numberOfAuthors = UserfileCommitsList.Select(ufc => ufc.Username).Distinct().Count();
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col\">");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><td class=\"text-right\">First commit</td><td class=\"text-right\">{analysis.FirstCommitDate.ToString("yyyy-MM-dd")}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Latest commit</td><td class=\"text-right\">{analysis.LatestCommitDate.ToString("yyyy-MM-dd")}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Number of commits</td><td class=\"text-right\">{totalCommits}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Lines of code analyzed</td><td class=\"text-right\">{analysis.LinesOfCodeanalyzed}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Number of authors</td><td class=\"text-right\">{numberOfAuthors}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Analysis time(milliseconds)</td><td class=\"text-right\">{analysis.AnalysisTime}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div></div>");
        }

        private void AddCommitsEachDayChartJavascript(StringBuilder sb, Dictionary<DateTime, int> commitsEachDay)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine($"var data = new google.visualization.DataTable();");
            sb.AppendLine($"data.addColumn('date', 'Date');");
            sb.AppendLine($"data.addColumn('number', 'Commits');");
            sb.AppendLine($"data.addColumn({{ type: 'string', role: 'tooltip'}});");
            sb.AppendLine($"data.addRows([");
            var dateOfFirstCommit = commitsEachDay.Keys.OrderBy(date => date).First();
            for (var date = dateOfFirstCommit; date <= DateTime.Now; date = date.AddDays(1))
            {
                var numberOfCommits = commitsEachDay.ContainsKey(date) ? commitsEachDay[date] : 0;
                sb.AppendLine($"      [new Date('{date.ToString("yyyy-MM-dd")}'), {numberOfCommits}, '{date.ToString("yyyy-MM-dd")}, {numberOfCommits}'],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { width: 1200, height: 500, legend: 'none', hAxis: { title: 'Date'}, vAxis: { title: 'Commits' } }; ");
            sb.AppendLine("   var element = document.getElementById('commitsEachDayChart');");
            sb.AppendLine("   element.classList.remove(\"spinner\");");
            sb.AppendLine("   element.style = \"width: 1200px; height: 500px;\"");
            sb.AppendLine("   var chart = new google.visualization.LineChart(element);");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionCommitsForEachDay(StringBuilder sb, Dictionary<DateTime, int> commitsEachDay)
        {
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col\">");
            sb.AppendLine("<h2>Commits for each day</h2>");
            AddCommitsEachDayChartJavascript(sb, commitsEachDay);
            sb.AppendLine("<div id=\"commitsEachDayChart\" class=\"spinner\"></div>");
            sb.AppendLine("</div></div>");
        }

        private void AddLinesChangedEachDayChartJavascript(StringBuilder sb, Dictionary<DateTime, int> linesOfCodeAddedEachDay, Dictionary<DateTime, int> linesOfCodeDeletedEachDay)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine($"var data = new google.visualization.DataTable();");
            sb.AppendLine($"data.addColumn('date', 'Date');");
            sb.AppendLine($"data.addColumn('number', 'Lines added');");
            sb.AppendLine($"data.addColumn('number', 'Lines deleted');");
            sb.AppendLine($"data.addColumn({{ type: 'string', role: 'tooltip'}});");
            sb.AppendLine($"data.addRows([");
            var dateOfFirstChange = linesOfCodeAddedEachDay.Keys.OrderBy(date => date).First();
            for (var date = dateOfFirstChange; date <= DateTime.Now; date = date.AddDays(1))
            {
                var numberOfLinesAdded = linesOfCodeAddedEachDay.ContainsKey(date) ? linesOfCodeAddedEachDay[date] : 0;
                var numberOfLinesDeleted = linesOfCodeDeletedEachDay.ContainsKey(date) ? linesOfCodeDeletedEachDay[date] : 0;
                sb.AppendLine($"      [new Date('{date.ToString("yyyy-MM-dd")}'), {numberOfLinesAdded}, {numberOfLinesDeleted}, '{date.ToString("yyyy-MM-dd")}, +{numberOfLinesAdded}, -{numberOfLinesDeleted}'],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { width: 1200, height: 500, hAxis: { title: 'Date'}, vAxis: { title: 'Lines added/deleted' } }; ");
            sb.AppendLine("   var element = document.getElementById('linesChangedEachDayChart');");
            sb.AppendLine("   element.classList.remove(\"spinner\");");
            sb.AppendLine("   element.style = \"width: 1200px; height: 500px;\"");
            sb.AppendLine("   var chart = new google.visualization.LineChart(element);");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionLinesChangedEachDay(StringBuilder sb, Dictionary<DateTime, int> linesOfCodeAddedEachDay, Dictionary<DateTime, int> linesOfCodeDeletedEachDay)
        {
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col\">");
            sb.AppendLine("<h2>Lines changed each day</h2>");
            sb.AppendLine();
            AddLinesChangedEachDayChartJavascript(sb, linesOfCodeAddedEachDay, linesOfCodeDeletedEachDay);
            sb.AppendLine("<div id=\"linesChangedEachDayChart\" class=\"spinner\"></div>");
            sb.AppendLine("</div></div>");
        }

        private void AddCodeAgeChartJavascript(StringBuilder sb, Dictionary<int, int> codeAge)
        {
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart']});");
            sb.AppendLine("google.charts.setOnLoadCallback(drawChart);");
            sb.AppendLine("function drawChart() {");
            sb.AppendLine($"var data = new google.visualization.DataTable();");
            sb.AppendLine($"data.addColumn('number', 'Code age');");
            sb.AppendLine($"data.addColumn('number', 'Filechanges');");
            sb.AppendLine($"data.addColumn({{ type: 'string', role: 'tooltip'}});");
            sb.AppendLine($"data.addColumn({{ type: 'string', role: 'annotation'}});");
            sb.AppendLine($"data.addRows([");
            var maxAge = codeAge.AsEnumerable().OrderByDescending(kvp => kvp.Key).First().Key;
            for (var month = 0; month <= maxAge; month++)
            {
                var fileChanges = codeAge.ContainsKey(month) ? codeAge[month] : 0;
                sb.AppendLine($"      [{month}, {fileChanges}, 'Age: {month} months, Changes: {fileChanges}', '{fileChanges}'],");
            }
            sb.AppendLine("   ]);");
            sb.AppendLine("   var options = { width: 1200, height: 500, legend: 'none', hAxis: { title: 'Codeage'}, vAxis: { title: 'Filechanges' } }; ");
            sb.AppendLine("   var element = document.getElementById('codeAgeChart');");
            sb.AppendLine("   element.classList.remove(\"spinner\");");
            sb.AppendLine("   element.style = \"width: 1200px; height: 500px;\"");
            sb.AppendLine("   var chart = new google.visualization.ColumnChart(element);");
            sb.AppendLine("   chart.draw(data, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionCodeAge(StringBuilder sb, Dictionary<int, int> codeAge)
        {
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col\">");
            sb.AppendLine("<h2>Code age</h2>");
            sb.AppendLine();
            AddCodeAgeChartJavascript(sb, codeAge);
            sb.AppendLine("<div id=\"codeAgeChart\" class=\"spinner\"></div>");
            sb.AppendLine("</div></div>");
        }

        private void AddSectionFileTypes(StringBuilder sb, Dictionary<string, int> fileTypes)
        {
            var fileTypesOrdered = fileTypes.AsEnumerable().OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key);
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col\">");
            sb.AppendLine("<h2>Number of files of each type</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: auto\">");
            sb.AppendLine("<tr><th class=\"text-right\">File type</th><th>Count</th></tr>");
            int rowCounter = 1;
            foreach (var kvp in fileTypesOrdered)
            {
                var icon = FileIcon("a" + kvp.Key);
                sb.AppendLine($"<tr><td class=\"text-right\"><i class=\"{icon} iw\"></i>{WebUtility.HtmlEncode(kvp.Key)}</td><td class=\"text-right\">{kvp.Value}</td></tr>");
                if (rowCounter++ % 10 == 0)
                {
                    sb.AppendLine("</table>");
                    sb.AppendLine("<table class=\"table pull-left\" style=\"width: auto\">");
                    sb.AppendLine("<tr><th class=\"text-right\">File type</th><th>Count</th></tr>");
                }
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div></div>");
        }

        private void AddSectionCommitsForEachFile(StringBuilder sb, FileStat fileChange, int sectionCounter)
        {
            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine($"<h3>{WebUtility.HtmlEncode(fileChange.Filename)}</h3>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><td class=\"text-right\">Latest commit</td><td>{fileChange.LatestCommit.ToString("yyyy-MM-dd")}</td></tr>");
            sb.AppendLine($"<tr><td class=\"text-right\">Commits</td><td>{fileChange.CommitCount}</td></tr>");
            if (fileChange.FileExists)
            {
                var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
                var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
                var methodCount = fileChange.MethodCount > 0 ? fileChange.MethodCount.ToString() : "N/A";
                sb.AppendLine($"<tr><td class=\"text-right\">Lines of code</td><td>{linesOfCode}</td></tr>");
                sb.AppendLine($"<tr><td class=\"text-right\">Cyclomatic Complexity</td><td>{cyclomaticComplexity}</td></tr>");
                sb.AppendLine($"<tr><td class=\"text-right\">Method count</td><td>{methodCount}</td></tr>");
            }
            else
            {
                sb.AppendLine($"<tr><td class=\"text-right\">File has been deleted</td><td></td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-6\">");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class=\"row\"><div class=\"col-md-6\">");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: 500px\">");
            sb.AppendLine($"<tr><th>Name</th><th>Commits</th><th>Percentage</th><th>Latest commit</th></tr>");
            var commitDates = new List<ScatterPoint>();
            foreach (var userfileChange in UserfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
            {
                var username = WebUtility.HtmlEncode(userfileChange.Username);
                var changeCount = string.Format("{0,3}", userfileChange.CommitCount);
                var percentage = string.Format("{0,5:#0.00}", ((double)userfileChange.CommitCount / (double)fileChange.CommitCount) * 100);
                var latestCommit = GenerateScatterPlotData(commitDates, userfileChange);
                sb.AppendLine($"<tr><td class=\"text-right\">{username}</td><td>{changeCount}</td><td>{percentage}%</td><td>{latestCommit}</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class=\"col-md-6\">");
            AddScatterplotJavascript(sb, commitDates, sectionCounter);
            sb.AppendLine($"    <div id=\"scatterChart{sectionCounter}\" class=\"spinner\"></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
        }

        private string GenerateScatterPlotData(List<ScatterPoint> commitDates, FileStat fileStat)
        {
            var commitDatesOrdered = fileStat.CommitDates.OrderBy(date => date);
            foreach (var commitDate in commitDatesOrdered)
            {
                var date = $"new Date('{commitDate.ToString("yyyy-MM-dd")}')";
                var userId = UserNameKey[fileStat.Username];
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
            var latestCommitDate = commitDatesOrdered.Last();
            return latestCommitDate.ToString("yyyy-MM-dd");
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
            sb.AppendLine($"var element = document.getElementById('scatterChart{sectionCounter}');");
            sb.AppendLine("element.classList.remove(\"spinner\");");
            sb.AppendLine("element.style = \"width: 900px; height: 300px;\"");
            sb.AppendLine("var chart = new google.visualization.ScatterChart(element);");
            sb.AppendLine($"chart.draw(data{sectionCounter}, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionTags(StringBuilder sb, Dictionary<DateTime, string> tags)
        {
            var tagsOrdered = tags.AsEnumerable().OrderByDescending(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col-md-4\">");
            sb.AppendLine("<h2>Tags</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: auto\">");
            sb.AppendLine("<tr><th>Tag</th><th>Commit date</th></tr>");
            var tagDates = new List<ScatterPoint>();
            foreach (var kvp in tagsOrdered)
            {
                var tag = kvp.Value;
                var tagCommitDate = kvp.Key.ToString("yyyy-MM-dd");
                sb.AppendLine($"<tr><td>{WebUtility.HtmlEncode(tag)}</td><td>{tagCommitDate}</td></tr>");
                var date = $"new Date('{tagCommitDate}')";
                tagDates.Add(new ScatterPoint
                {
                    Date = date,
                    UserId = 1,
                    ToolTip = $"'{tag}, {tagCommitDate}'"
                });
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"col-md-8\">");
            AddTagsScatterplotJavascript(sb, tagDates);
            sb.AppendLine("    <div id=\"scatterChartTags\" \" class=\"spinner\"></div>");
            sb.AppendLine("</div></div>");
        }

        private static void AddTagsScatterplotJavascript(StringBuilder sb, List<ScatterPoint> tagDates)
        {
            var tagDatesString = string.Join(",", tagDates.Select(sp => sp.ToString()).OrderBy(sp => sp));

            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("google.charts.load('current', { 'packages':['corechart'] });");
            sb.AppendLine($"google.charts.setOnLoadCallback(drawChartTags);");
            sb.AppendLine($"function drawChartTags() {{");
            sb.AppendLine($"var dataTags = new google.visualization.DataTable();");
            sb.AppendLine($"dataTags.addColumn('date', 'Date');");
            sb.AppendLine($"dataTags.addColumn('number', 'Tag');");
            sb.AppendLine($"dataTags.addColumn({{ type: 'string', role: 'tooltip'}});");
            sb.AppendLine($"dataTags.addRows([");
            sb.AppendLine(tagDatesString);
            sb.AppendLine("]);");
            sb.AppendLine("var options = { width: 900, height: 300, title: 'Tags', legend: 'none' };");
            sb.AppendLine("var element = document.getElementById('scatterChartTags');");
            sb.AppendLine("element.classList.remove(\"spinner\");");
            sb.AppendLine("element.style = \"width: 900px; height: 300px;\"");
            sb.AppendLine("var chart = new google.visualization.ScatterChart(element);");
            sb.AppendLine($"var chart = new google.visualization.ScatterChart(document.getElementById('scatterChartTags'));");
            sb.AppendLine($"chart.draw(dataTags, options);");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
        }

        private void AddSectionBranches(StringBuilder sb, List<string> branches)
        {
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"col-md-4\">");
            sb.AppendLine("<h2>Branches</h2>");
            sb.AppendLine("<table class=\"table pull-left\" style=\"width: auto\">");
            sb.AppendLine("<tr><th>Name</th></tr>");
            foreach (var branch in branches)
            {
                sb.Append($"<tr><td>{branch}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div></div>");
        }
    }
}
