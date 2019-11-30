using OfficeOpenXml;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommitsAnalysis.Reporting
{
    public class ExcelReport : BaseReport, IReport
    {
        private readonly ISystemIO systemIO;
        public ExcelReport(ISystemIO systemIO, string reportFilename, Options options) : base(systemIO, reportFilename, options)
        {
            this.systemIO = systemIO;
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating Excel report...");
            if (analysis == null) throw new ArgumentException("Parameter analysis is null.", nameof(analysis));
            this.FileCommitsList = analysis.FileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename);
            this.UserfileCommitsList = analysis.UserfileCommits.Values.OrderByDescending(fc => fc.CommitCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
            this.FolderCommits = analysis.FolderCommits;
            this.FolderCommitsList = analysis.FolderCommits.Values.OrderByDescending(fc => fc.FileChanges);

            using (var excelPackage = new ExcelPackage())
            {
                var sheetCommitsForEachSubfolder = excelPackage.Workbook.Worksheets.Add("Commits for each sub-folder");
                AddSectionCommitsForEachMonth(sheetCommitsForEachSubfolder);

                var sheetTopMostChangedFiles = excelPackage.Workbook.Worksheets.Add($"Top {NumberOfFilesToList} most changed files");
                AddSectionCommitsForEachFile(sheetTopMostChangedFiles);

                var sheetStatistics = excelPackage.Workbook.Worksheets.Add("Statistics");
                AddSectionStatistics(sheetStatistics, analysis);

                var sheetCommitsEachDay = excelPackage.Workbook.Worksheets.Add("Commits each day");
                AddSectionCommitsEachDay(sheetCommitsEachDay, analysis.CommitsEachDay);

                var sheetLinesChangedEachDay = excelPackage.Workbook.Worksheets.Add("Lines changed each day");
                AddSectionLinesChangedEachDay(sheetLinesChangedEachDay, analysis.LinesOfCodeAddedEachDay, analysis.LinesOfCodeDeletedEachDay);

                if (analysis.Tags.Any() || analysis.Branches.Any())
                {
                    var sheetTags = excelPackage.Workbook.Worksheets.Add("Tags and Branches");
                    AddSectionTagsAndBranches(sheetTags, analysis.Tags, analysis.Branches);
                }

                var sheetNumberOfFilesOfEachType = excelPackage.Workbook.Worksheets.Add("Number of files of each type");
                AddSectionNumberOfFilesOfEachType(sheetNumberOfFilesOfEachType, analysis.FileTypes);

                excelPackage.SaveAs(systemIO.FileInfo($"{ReportFilename}.xlsx"));
            }
        }


        private void AddSectionCommitsForEachMonth(ExcelWorksheet sheet)
        {
            Header(sheet, "Commits for each sub-folder");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Folder", 40);
            sheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            TableHeader(sheet, rowCounter, 2, "File changes", 13);
            sheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            TableHeader(sheet, rowCounter, 3, "Percentage", 10);
            sheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Column(3).Style.Numberformat.Format = "#,##0.00%";

            rowCounter++;
            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            foreach (var folder in FolderCommitsList.Take(25))
            {
                sheet.Cells[rowCounter, 1].Value = folder.FolderName;
                sheet.Cells[rowCounter, 2].Value = FolderCommits[folder.FolderName].FileChanges;
                var percentage = (double)FolderCommits[folder.FolderName].FileChanges / (double)totalCommits;
                sheet.Cells[rowCounter, 3].Value = percentage;
                rowCounter++;
            }
            var chart = sheet.Drawings.AddChart("Commits each day", OfficeOpenXml.Drawing.Chart.eChartType.Pie);
            chart.SetSize(500, 500);
            chart.SetPosition(0, 450);
            var series1 = chart.Series.Add($"$B$4:$B${rowCounter}", $"$A$4:$A${rowCounter}");
        }

        private void AddSectionCommitsForEachFile(ExcelWorksheet sheet)
        {
            Header(sheet, $"Top {NumberOfFilesToList} most changed files");

            int rowCounter = 3;
            foreach (var fileChange in FileCommitsList.Take(NumberOfFilesToList))
            {
                rowCounter = AddSectionCommitsForFile(sheet, fileChange, rowCounter);
            }
        }

        private int AddSectionCommitsForFile(ExcelWorksheet sheet, FileStat fileChange, int rowCounter)
        {
            sheet.Cells[rowCounter, 1].Value = fileChange.Filename;
            sheet.Cells[rowCounter, 1].Style.Font.Size = 16;
            rowCounter++;

            var latestFileCommit = fileChange.CommitDates.OrderByDescending(cd => cd).First();
            sheet.Cells[rowCounter, 1].Value = "Latest commit";
            sheet.Cells[rowCounter, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Cells[rowCounter, 2].Value = latestFileCommit;
            rowCounter++; sheet.Cells[rowCounter, 1].Value = "Commits";
            sheet.Cells[rowCounter, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Cells[rowCounter, 2].Value = fileChange.CommitCount;
            rowCounter++;
            var linesOfCode = fileChange.LinesOfCode > 0 ? fileChange.LinesOfCode.ToString() : "N/A";
            sheet.Cells[rowCounter, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Cells[rowCounter, 2].Value = linesOfCode;
            rowCounter++;
            var cyclomaticComplexity = fileChange.CyclomaticComplexity > 0 ? fileChange.CyclomaticComplexity.ToString() : "N/A";
            sheet.Cells[rowCounter, 1].Value = "Cyclomatic complexity";
            sheet.Cells[rowCounter, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Cells[rowCounter, 2].Value = cyclomaticComplexity;
            rowCounter++;
            var methodCount = fileChange.MethodCount > 0 ? fileChange.MethodCount.ToString() : "N/A"; sheet.Cells[rowCounter, 1].Value = "Lines of code";
            sheet.Cells[rowCounter, 1].Value = "Method count";
            sheet.Cells[rowCounter, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Cells[rowCounter, 2].Value = methodCount;
            rowCounter++;

            TableHeader(sheet, rowCounter, 1, "Author", 25);
            TableHeader(sheet, rowCounter, 2, "Commits", 9);
            TableHeader(sheet, rowCounter, 3, "Percentage", 11);
            TableHeader(sheet, rowCounter, 4, "Latest commit", 11);
            rowCounter++;
            foreach (var userfileChange in UserfileCommitsList.Where(ufc => ufc.Filename == fileChange.Filename))
            {
                sheet.Cells[rowCounter, 1].Value = userfileChange.Username;
                sheet.Cells[rowCounter, 2].Value = userfileChange.CommitCount;
                sheet.Cells[rowCounter, 3].Value = (double)userfileChange.CommitCount / (double)fileChange.CommitCount;
                sheet.Cells[rowCounter, 3].Style.Numberformat.Format = "#,##0.00%";
                var commitDatesOrdered = userfileChange.CommitDates.OrderBy(date => date);
                sheet.Cells[rowCounter, 4].Value = commitDatesOrdered.Last().ToString("yyyy-MM-dd");
                rowCounter++;
            }

            return rowCounter;
        }


        private void AddSectionStatistics(ExcelWorksheet sheet, Analysis analysis)
        {
            Header(sheet, "Statistics");

            sheet.Column(1).Width = 24;
            sheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            sheet.Column(2).Width = 11;
            sheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            int rowCounter = 3;
            sheet.Cells[rowCounter, 1].Value = "First commit";
            sheet.Cells[rowCounter, 2].Value = analysis.FirstCommitDate.ToString("yyyy-MM-dd");
            rowCounter++;
            sheet.Cells[rowCounter, 1].Value = "Latest commit";
            sheet.Cells[rowCounter, 2].Value = analysis.LatestCommitDate.ToString("yyyy-MM-dd");
            rowCounter++;
            var totalCommits = FileCommitsList.Sum(fc => fc.CommitCount);
            sheet.Cells[rowCounter, 1].Value = "Number of commits";
            sheet.Cells[rowCounter, 2].Value = totalCommits;
            rowCounter++;
            sheet.Cells[rowCounter, 1].Value = "Lines of code analyzed";
            sheet.Cells[rowCounter, 2].Value = analysis.LinesOfCodeanalyzed;
            rowCounter++;
            var numberOfAuthors = UserfileCommitsList.Select(ufc => ufc.Username).Distinct().Count();
            sheet.Cells[rowCounter, 1].Value = "Number of authors";
            sheet.Cells[rowCounter, 2].Value = numberOfAuthors;
            rowCounter++;
            sheet.Cells[rowCounter, 1].Value = "Analysis time(milliseconds)";
            sheet.Cells[rowCounter, 2].Value = analysis.AnalysisTime;
            rowCounter++;
        }

        private void AddSectionCommitsEachDay(ExcelWorksheet sheet, Dictionary<DateTime, int> commitsEachDay)
        {
            Header(sheet, "Commits each day");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Date", 11);
            TableHeader(sheet, rowCounter, 2, "Commits", 10);

            rowCounter++;
            var dateOfFirstChange = commitsEachDay.Keys.OrderBy(date => date).First();
            for (var date = dateOfFirstChange; date <= DateTime.Now; date = date.AddDays(1))
            {
                var commits = commitsEachDay.ContainsKey(date) ? commitsEachDay[date] : 0;
                sheet.Cells[rowCounter, 1].Value = date.ToString("yyyy-MM-dd");
                sheet.Cells[rowCounter, 2].Value = commits;
                rowCounter++;
            }
            var chart = sheet.Drawings.AddChart("Commits each day", OfficeOpenXml.Drawing.Chart.eChartType.Line);
            chart.SetSize(600, 400);
            chart.SetPosition(0, 200);
            var series1 = chart.Series.Add($"$B$4:$B${rowCounter}", $"$A$4:$A${rowCounter}");
            series1.Header = "Commits";
            chart.Legend.Remove();
        }

        private void AddSectionLinesChangedEachDay(ExcelWorksheet sheet, Dictionary<DateTime, int> linesOfCodeAddedEachDay, Dictionary<DateTime, int> linesOfCodeDeletedEachDay)
        {
            Header(sheet, "Lines changed each day");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Date", 11);
            TableHeader(sheet, rowCounter, 2, "Lines Added", 12);
            TableHeader(sheet, rowCounter, 3, "Lines deleted", 12);

            rowCounter++;
            var dateOfFirstChange = linesOfCodeAddedEachDay.Keys.OrderBy(date => date).First();
            for (var date = dateOfFirstChange; date <= DateTime.Now; date = date.AddDays(1))
            {
                var numberOfLinesAdded = linesOfCodeAddedEachDay.ContainsKey(date) ? linesOfCodeAddedEachDay[date] : 0;
                var numberOfLinesDeleted = linesOfCodeDeletedEachDay.ContainsKey(date) ? linesOfCodeDeletedEachDay[date] : 0;
                sheet.Cells[rowCounter, 1].Value = date.ToString("yyyy-MM-dd");
                sheet.Cells[rowCounter, 2].Value = numberOfLinesAdded;
                sheet.Cells[rowCounter, 3].Value = numberOfLinesDeleted;
                rowCounter++;
            }
            var chart = sheet.Drawings.AddChart("LineOfCodeChangeEachDay", OfficeOpenXml.Drawing.Chart.eChartType.Line);
            chart.SetSize(600, 400);
            chart.SetPosition(0, 250);
            var series1 = chart.Series.Add($"$B$4:$B${rowCounter}", $"$A$4:$A${rowCounter}");
            series1.Header = "Added";
            var series2 = chart.Series.Add($"$C$4:$C${rowCounter}", $"$A$4:$A${rowCounter}");
            series2.Header = "Deleted";
            chart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Top;
        }

        private void AddSectionTagsAndBranches(ExcelWorksheet sheet, Dictionary<DateTime, string> tags, List<string> branches)
        {
            var tagsOrdered = tags.AsEnumerable().OrderByDescending(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
            Header(sheet, "Tags");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Tag", 24);
            TableHeader(sheet, rowCounter, 2, "Commit date", 11);

            rowCounter++;
            foreach (var kvp in tagsOrdered)
            {
                var tag = kvp.Value;
                var tagCommitDate = kvp.Key.ToString("yyyy-MM-dd");
                sheet.Cells[rowCounter, 1].Value = tag;
                sheet.Cells[rowCounter, 2].Value = tagCommitDate;
                rowCounter++;
            }

            Header(sheet, "Branches", 1, 4);

            rowCounter = 3;
            TableHeader(sheet, rowCounter, 4, "Branch", 24);

            rowCounter++;
            foreach (var branch in branches)
            {
                sheet.Cells[rowCounter, 4].Value = branch;
                rowCounter++;
            }
        }

        private void AddSectionNumberOfFilesOfEachType(ExcelWorksheet sheet, Dictionary<string, int> fileTypes)
        {
            Header(sheet, "Number of files of each type");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "File type");
            TableHeader(sheet, rowCounter, 2, "Count");

            rowCounter++;
            var fileTypesOrdered = fileTypes.AsEnumerable().OrderByDescending(kvp => kvp.Value).ThenBy(kvp => kvp.Key);
            foreach (var kvp in fileTypesOrdered)
            {
                sheet.Cells[rowCounter, 1].Value = kvp.Key;
                sheet.Cells[rowCounter, 2].Value = kvp.Value;
                rowCounter++;
            }
        }

        private void Header(ExcelWorksheet sheet, string header, int row = 1, int column = 1)
        {
            sheet.Cells[row, column].Value = header;
            sheet.Cells[row, column].Style.Font.Size = 18;
        }

        private void TableHeader(ExcelWorksheet sheet, int row, int col, string text, int width = -1)
        {
            sheet.Cells[row, col].Value = text;
            sheet.Cells[row, col].Style.Font.Bold = true;
            if (width > 0)
            {
                sheet.Column(col).Width = width;
            }
        }
    }
}
