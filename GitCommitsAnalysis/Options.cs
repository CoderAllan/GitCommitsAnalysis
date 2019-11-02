using System.Collections.Generic;
using CommandLine;

namespace GitCommitsAnalysis
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

        [Option('n', "numberoffiletolist", Default = 50, HelpText = "Specifies the number of flies to include in the list of most changes files. (Ignored when output is Json)")]
        public int NumberOfFilesInList { get; set; }
    }

    public enum OutputFormat
    {
        Text,
        Markdown,
        Json,
        HTML
    }
}
