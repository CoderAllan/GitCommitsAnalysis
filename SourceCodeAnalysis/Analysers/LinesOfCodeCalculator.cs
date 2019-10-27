using System;
using System.Linq;

namespace SourceCodeAnalysis.Analysers
{

    public class LinesOfCodeCalculator
    {
        public int Calculate(string fileContents)
        {
            var lines = fileContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return lines.Where(l => !l.Trim().StartsWith("using")).ToList().Count();
        }

    }
}