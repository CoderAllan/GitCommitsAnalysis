using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Reporting;
using System.Collections.Generic;

namespace SourceCodeAnalysis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fileHandling = new FileHandling();
            var textFileReport = new TextFileReport(fileHandling, @"D:\Temp\SourceCodeAnalysisReport.txt");
            var markdownReport = new MarkdownReport(fileHandling, @"D:\Temp\SourceCodeAnalysisReport.md");
            
            var reports = new List<IReport> { textFileReport, markdownReport};
            var sourceCodeAnalysis = new SourceCodeAnalysis(fileHandling, reports.ToArray());
            
            var rootFolder = "D:\\Src\\AssetValueService";
            sourceCodeAnalysis.PerformAnalysis(rootFolder);
        }

    }

}
