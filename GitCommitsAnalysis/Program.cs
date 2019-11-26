using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Reporting;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using System;

namespace GitCommitsAnalysis
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var parser = new Parser(with => with.HelpWriter = null))
            {
                var parserResult = parser.ParseArguments<Options>(args);
                parserResult.WithParsed(options =>
                {
                    var systemIO = new SystemIO();
                    var reports = getReportGenerators(options, systemIO);
                    var GitCommitsAnalysis = new GitCommitsAnalysis(systemIO, reports, options);

                    GitCommitsAnalysis.PerformAnalysis(options.RootFolder);
                }).WithNotParsed(x =>
                {
                    var helpText = HelpText.AutoBuild(parserResult, h =>
                    {
                        h.AutoHelp = false; //hide --help
                        h.AutoVersion = false;   //hide --version	
                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                    }, e => e);
                    Console.WriteLine(helpText);
                });
            }
        }

        private static List<IReport> getReportGenerators(Options options, ISystemIO systemIO)
        {
            var reportGenerators = new List<IReport>();
            foreach (var format in options.OutputFormat)
            {
                string outputFilename = "GitCommitsAnalysisReport";
                if (!string.IsNullOrEmpty(options.ReportFilename))
                {
                    outputFilename = systemIO.GetPathWitoutExtension(options.ReportFilename);
                }
                var filename = $"{options.OutputFolder}\\{outputFilename}";
                if (format == OutputFormat.Text)
                {
                    reportGenerators.Add(new TextFileReport(systemIO, filename, options));
                }
                if (format == OutputFormat.Markdown)
                {
                    reportGenerators.Add(new MarkdownReport(systemIO, filename, options));
                }
                if (format == OutputFormat.Json)
                {
                    reportGenerators.Add(new JsonReport(systemIO, filename, options));
                }
                if (format == OutputFormat.HTML)
                {
                    reportGenerators.Add(new HTMLReport(systemIO, filename, options));
                }
                if (format == OutputFormat.Excel)
                {
                    reportGenerators.Add(new ExcelReport(systemIO, filename, options));
                }
            }
            return reportGenerators;
        }
    }
}
