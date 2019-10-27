using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Model;

namespace SourceCodeAnalysis.Reporting
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

        public void Generate(Dictionary<string, FileStat> fileChanges, Dictionary<string, FileStat> userfileChanges, Dictionary<string, FileStat> folderChanges)
        {
            throw new NotImplementedException();
        }
    }
}
