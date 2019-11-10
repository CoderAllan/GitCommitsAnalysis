using GitCommitsAnalysis.Model;

namespace GitCommitsAnalysis.Interfaces
{
    public interface IReport
    {
        void Generate(Analysis analysis);
    }
}
