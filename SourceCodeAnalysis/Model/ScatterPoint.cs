namespace SourceCodeAnalysis.Model
{
    public class ScatterPoint
    {
        public string Date { get; set; }
        public int UserId { get; set; }
        public string ToolTip { get; set; }

        public override string ToString()
        {
            return $"[{Date},{UserId},{ToolTip}]";
        }
    }
}
