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
        public JsonReport(ISystemIO systemIO, string reportFilename, string title)
        {
            this.reportFilename = reportFilename;
            this.title = title;
            this.systemIO = systemIO;
        }

        public void Generate(Analysis analysis)
        {
            Console.WriteLine("Generating Json file...");
            systemIO.WriteAllText($"{reportFilename}.json", JsonConvert.SerializeObject(new {
                title,
                analysis.FileCommits,
                analysis.UserfileCommits,
                analysis.FolderCommits,
                analysis.CommitsEachDay,
                analysis.LinesOfCodeAddedEachDay,
                analysis.LinesOfCodeDeletedEachDay
            }));
        }
    }
}
