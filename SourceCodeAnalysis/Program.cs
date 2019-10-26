using SourceCodeAnalysis.Reporting;

namespace SourceCodeAnalysis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fileHandling = new FileHandling();
            var textFileReport = new TextFileReport(@"D:\Temp\SourceCodeAnalysisReport.txt");
            var sourceCodeAnalysis = new SourceCodeAnalysis(fileHandling, textFileReport);
            
            var rootFolder = "D:\\Src\\AssetValueService";
            sourceCodeAnalysis.PerformAnalysis(rootFolder);
        }

    }

}
