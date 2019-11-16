using System.Collections.Generic;

namespace GitCommitsAnalysis.Model
{
    public class FolderStat
    {
        public FolderStat(string folderName, int fileChanges)
        {
            FolderName = folderName;
            FileChanges = fileChanges;
        }

        public bool IsRoot { get; set; } = false;
        public string FolderName { get; set; }
        public int FileChanges { get; set; } = 0;
        public Dictionary<string, FolderStat> Children { get; } = new Dictionary<string, FolderStat>();
    }
}
