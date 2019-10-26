namespace SourceCodeAnalysis.Model
{
    public class UsernameFilename
    {
        public string Username { get; set; }
        public string Filename { get; set; }
        public int? CyclomaticComplexity { get; set; } = null;
        public int? LinesOfCode { get; set; } = null;

        public string DictKey { get { return Filename + Username; } }
    }
}
