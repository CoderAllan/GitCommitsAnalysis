using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Reporting;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using System;

namespace SourceCodeAnalysis
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var parser = new Parser(with => with.HelpWriter = null))
            {
                var parserResult = parser.ParseArguments<Options>(args);
                parserResult.WithParsed(opts =>
                {
                    var systemIO = new SystemIO();
                    var reports = getReportGenerators(opts, systemIO);
                    var sourceCodeAnalysis = new SourceCodeAnalysis(systemIO, reports);

                    sourceCodeAnalysis.PerformAnalysis(opts.RootFolder);
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

        private static List<IReport> getReportGenerators(Options opts, ISystemIO systemIO)
        {
            var reportGenerators = new List<IReport>();
            foreach (var format in opts.OutputFormat)
            {
                string outputFilename = "SourcecodeAnalysisReport";
                if (!string.IsNullOrEmpty(opts.ReportFilename))
                {
                    outputFilename = systemIO.GetPathWitoutExtension(opts.ReportFilename);
                }
                var filename = $"{opts.OutputFolder}\\{outputFilename}";
                if (format == OutputFormat.Text)
                {
                    reportGenerators.Add(new TextFileReport(systemIO, filename));
                }
                if (format == OutputFormat.Markdown)
                {
                    reportGenerators.Add(new MarkdownReport(systemIO, filename));
                }
                if (format == OutputFormat.Json)
                {
                    reportGenerators.Add(new JsonReport(systemIO, filename));
                }
                if (format == OutputFormat.HTML)
                {
                    reportGenerators.Add(new HTMLReport(systemIO, filename));
                }
            }
            return reportGenerators;
        }
    }
}
