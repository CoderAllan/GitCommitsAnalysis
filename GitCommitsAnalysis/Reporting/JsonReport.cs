using System;
using System.Collections.Generic;
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

        public void Generate(
            Dictionary<string, FileStat> fileCommits,
            Dictionary<string, FileStat> userfileCommits,
            Dictionary<string, FileStat> folderCommits,
            Dictionary<DateTime, int> commitsEachDay,
            Dictionary<DateTime, int> linesOfCodeAddedEachDay,
            Dictionary<DateTime, int> linesOfCodeDeletedEachDay
            )
        {
            Console.WriteLine("Generating Json file...");
            systemIO.WriteAllText($"{reportFilename}.json", JsonConvert.SerializeObject(new {
                title,
                fileCommits,
                userfileCommits,
                folderCommits,
                commitsEachDay,
                linesOfCodeAddedEachDay,
                linesOfCodeDeletedEachDay
            }));
        }
    }
}
