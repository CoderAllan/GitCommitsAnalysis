using System;
using Newtonsoft.Json;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Reporting
{
    public class JsonReport : IReport
    {
        private string reportFilename;
        private string title;
        private ISystemIO systemIO;
        public JsonReport(ISystemIO systemIO, string reportFilename, Options options)
        {
            this.reportFilename = reportFilename;
            this.title = options.Title;
            this.systemIO = systemIO;
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating Json file...");
            systemIO.WriteAllText($"{reportFilename}.json", JsonConvert.SerializeObject(new {
                title,
                analysis
            }));
        }
    }
}
