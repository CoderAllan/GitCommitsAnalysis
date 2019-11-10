namespace GitCommitsAnalysis.Model
{

    public class FileStat : UsernameFilename
    {
        public int CommitCount { get; set; } = 1;
        public int MethodCount { get; set; }
    }
}
