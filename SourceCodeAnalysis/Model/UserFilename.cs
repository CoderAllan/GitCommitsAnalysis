using System;
using System.Collections.Generic;

namespace SourceCodeAnalysis.Model
{
    public class UsernameFilename
    {
        public string Username { get; set; }
        public string Filename { get; set; }
        public List<DateTime> CommitDates { get; } = new List<DateTime>();
        public int? CyclomaticComplexity { get; set; } = null;
        public int? LinesOfCode { get; set; } = null;

        public static string GetDictKey(string filename, string username) { 
            return filename + "*" + username; 
        }
    }
}
