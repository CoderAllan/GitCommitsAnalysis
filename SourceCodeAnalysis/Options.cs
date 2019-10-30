using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace SourceCodeAnalysis
{
    public class Options
    {
        [Option('r', "rootfolder", Required = true, HelpText = "The root folder of the application source code")]
        public string RootFolder { get; set; }

        [Option('o', "outputfolder", Required = true, HelpText = "The output folder where the generated reports will be placed")]
        public string OutputFolder { get; set; }

        [Option('a', "reportfilename", Required = false, HelpText = "The filename the report(s) will be given")]
        public string ReportFilename { get; set; }

        [Option('f', "outputformat", Required = false, HelpText = "The output format(s) to generate. Multiple formats should be space-seperated. Eg. '-f Text Json'")]
        public IEnumerable<OutputFormat> OutputFormat { get; set; }
    }

    public enum OutputFormat
    {
        Text,
        Markdown,
        Json,
        HTML
    }
}
