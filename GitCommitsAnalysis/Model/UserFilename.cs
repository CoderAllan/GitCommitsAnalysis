using System;
using System.Collections.Generic;

namespace GitCommitsAnalysis.Model
{
    public class UsernameFilename
    {
        public string Username { get; set; }
        public string Filename { get; set; }
        public List<DateTime> CommitDates { get; } = new List<DateTime>();
        public int? CyclomaticComplexity { get; set; } = null;
        public int? LinesOfCode { get; set; } = null;
        public bool FileExists { get; set; } = false;
        public DateTime LatestCommit { get; set; } = DateTime.MinValue;
        public int CodeAge
        {
            get
            {
                var now = DateTime.Now;
                int monthsApart = 12 * (now.Year - LatestCommit.Year) + now.Month - LatestCommit.Month;
                return Math.Abs(monthsApart);
            }
        }

        public static string GetDictKey(string filename, string username)
        {
            return filename + "*" + username;
        }
    }
}
