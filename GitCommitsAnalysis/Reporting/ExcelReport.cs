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

            var excelPackage = new ExcelPackage();
            var sheetCommitsForEachSubfolder = excelPackage.Workbook.Worksheets.Add("Commits for each subfolder");
            AddSectionCommitsForEachMonth(sheetCommitsForEachSubfolder);
            var sheetTopMostChangedFiles = excelPackage.Workbook.Worksheets.Add($"Top {NumberOfFilesToList} most changed files");
            foreach (var fileChange in FileCommitsList.Take(NumberOfFilesToList))
            {
                AddSectionCommitsForEachFile(sheetTopMostChangedFiles, fileChange);
            }
            var sheetStatistics = excelPackage.Workbook.Worksheets.Add("Statistics");
            AddSectionStatistics(sheetStatistics, analysis);

            var sheetLinesChangedEachDay = excelPackage.Workbook.Worksheets.Add("Lines changed each day");
            AddSectionLinesChangedEachDay(sheetLinesChangedEachDay, analysis.LinesOfCodeAddedEachDay, analysis.LinesOfCodeDeletedEachDay);

            var sheetTags = excelPackage.Workbook.Worksheets.Add("Tags");
            AddSectionTags(sheetTags, analysis.Tags);

            var sheetNumberOfFilesOfEachType = excelPackage.Workbook.Worksheets.Add("Number of files of each type");
            AddSectionNumberOfFilesOfEachType(sheetNumberOfFilesOfEachType, analysis.FileTypes);

            //excelPackage.File =
            excelPackage.SaveAs(systemIO.FileInfo($"{ReportFilename}.xlsx"));
        }


        private void AddSectionCommitsForEachMonth(ExcelWorksheet sheet)
        {
            Header(sheet, "Commits for each subfolder");

            int rowCounter = 3;
            sheet.Cells[rowCounter, 1].Value = "";
            rowCounter++;

        }

        private void AddSectionCommitsForEachFile(ExcelWorksheet sheet, FileStat fileChange)
        {
            Header(sheet, $"Top {NumberOfFilesToList} most changed files");

            int rowCounter = 3;
        }

        private void AddSectionStatistics(ExcelWorksheet sheet, Analysis analysis)
        {
            Header(sheet, "Statistics");

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
            sheet.Cells[rowCounter, 1].Value = "Lines of code analysed";
            sheet.Cells[rowCounter, 2].Value = analysis.LinesOfCodeAnalysed;
            rowCounter++;
            var numberOfAuthors = UserfileCommitsList.Select(ufc => ufc.Username).Distinct().Count();
            sheet.Cells[rowCounter, 1].Value = "Number of authors";
            sheet.Cells[rowCounter, 2].Value = numberOfAuthors;
            rowCounter++;
            sheet.Cells[rowCounter, 1].Value = "Analysis time(milliseconds)";
            sheet.Cells[rowCounter, 2].Value = analysis.AnalysisTime;
            rowCounter++;
        }

        private void AddSectionLinesChangedEachDay(ExcelWorksheet sheet, Dictionary<DateTime, int> linesOfCodeAddedEachDay, Dictionary<DateTime, int> linesOfCodeDeletedEachDay)
        {
            Header(sheet, "Lines changed each day");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Date");
            TableHeader(sheet, rowCounter, 2, "Lines Added");
            TableHeader(sheet, rowCounter, 3, "Lines deleted");

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
        }

        private void AddSectionTags(ExcelWorksheet sheet, Dictionary<DateTime, string> tags)
        {
            var tagsOrdered = tags.AsEnumerable().OrderByDescending(kvp => kvp.Key).ThenBy(kvp => kvp.Value);
            Header(sheet, "Tags");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Tag");
            TableHeader(sheet, rowCounter, 2, "Commit date");

            rowCounter++;
            foreach (var kvp in tagsOrdered)
            {
                var tag = kvp.Value;
                var tagCommitDate = kvp.Key.ToString("yyyy-MM-dd");
                sheet.Cells[rowCounter, 1].Value = tag;
                sheet.Cells[rowCounter, 2].Value = tagCommitDate;
                rowCounter++;
            }
        }

        private void AddSectionNumberOfFilesOfEachType(ExcelWorksheet sheet, Dictionary<string, int> fileTypes)
        {
            Header(sheet, "Number of files of each type");

            int rowCounter = 3;
            TableHeader(sheet, rowCounter, 1, "Filetype");
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

        private void Header(ExcelWorksheet sheet, string header)
        {
            sheet.Cells[1, 1].Value = header;
            sheet.Cells[1, 1].Style.Font.Size = 18;
        }

        private void TableHeader(ExcelWorksheet sheet, int row, int col, string text)
        {
            sheet.Cells[row, col].Value = text;
            sheet.Cells[row, col].Style.Font.Bold = true;
        }
    }
}
