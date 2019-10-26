using System;


namespace SourceCodeAnalysis.Analysers
{
    public class SimpleLinesOfCodeCalculator
    {
        public int Calculate(string fileContents)
        {
            var lines = fileContents.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            return lines.Length;
        }
    }
}
