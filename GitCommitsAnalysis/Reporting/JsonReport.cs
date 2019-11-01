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
        private ISystemIO systemIO;
        public JsonReport(ISystemIO systemIO, string reportFilename)
        {
            this.systemIO = systemIO;
            this.reportFilename = reportFilename;
        }

        public void Generate(Dictionary<string, FileStat> fileCommits, Dictionary<string, FileStat> userfileCommits, Dictionary<string, FileStat> folderCommits)
        {
            Console.WriteLine("Generating Json file...");
            systemIO.WriteAllText($"{reportFilename}.json", JsonConvert.SerializeObject(new { fileCommits, userfileCommits, folderCommits }));
        }
    }
}
