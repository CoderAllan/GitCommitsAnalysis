using System;
using System.Linq;
using System.IO;

namespace GitCommitsAnalysis.Analysers
{

    public class LinesOfCodeCalculator
    {
        public int Calculate(string fileContents)
        {
            var lines = fileContents.Split(new string[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Where(l => !l.Trim().StartsWith("using")).ToList().Count();
        }

    }
}