using LibGit2Sharp;
using SourceCodeAnalysis.Analysers;
using SourceCodeAnalysis.Model;
using SourceCodeAnalysis.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace SourceCodeAnalysis
{
    public class SourceCodeAnalysis
    {
        private ISystemIO fileHandling;
        private IReport[] reports;

        public SourceCodeAnalysis(ISystemIO fileHandling, IReport[] reports)
        {
            this.fileHandling = fileHandling;
            this.reports = reports;
        }

        public void PerformAnalysis(string rootFolder)
        {
            using (var repo = new Repository(rootFolder))
            {
                var fileChanges = new Dictionary<string, FileStat>();
                var folderChanges = new Dictionary<string, FileStat>();
                var userfileChanges = new Dictionary<string, FileStat>();
                var cyclomaticComplexityCounter = new CyclomaticComplexityCounter();
                var linesOfCodeCalculator = new LinesOfCodeCalculator();
                var simpleLinesOfCodeCalculator = new SimpleLinesOfCodeCalculator();
                foreach (var commit in repo.Commits)
                {
                    //Console.WriteLine($"{counter} commit: {commit.Author.Name}, {commit.MessageShort}");
                    var username = commit.Author.Name;
                    foreach (var parent in commit.Parents)
                    {
                        foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                        {
                            var filename = change.Path;
                            int linesOfCode = 0;
                            int cyclomaticComplexity = 0;
                            var fullPath = Path.Combine(rootFolder, filename);
                            if (fileHandling.FileExists(fullPath))
                            {
                                //Console.WriteLine("Filename:" + filename);
                                var fileContents = fileHandling.ReadFileContent(fullPath);
                                if (filename.EndsWith(".cs"))
                                {
                                    //var cyclomaticComplexity = cyclomaticComplexityCounter.Calculate(fileContents);
                                    linesOfCode = linesOfCodeCalculator.Calculate(fileContents);
                                }
                                else
                                {
                                    linesOfCode = simpleLinesOfCodeCalculator.Calculate(fileContents);
                                }
                            }
                            if (fileChanges.ContainsKey(change.OldPath))
                            {
                                fileChanges[change.OldPath].ChangeCount++;
                                fileChanges[change.OldPath].LinesOfCode = linesOfCode;
                                if (change.Path != change.OldPath)
                                {
                                    // Console.WriteLine($"Filename old: {change.OldPath} - Path: {change.Path}");
                                    fileChanges[filename] = fileChanges[change.OldPath];
                                    fileChanges.Remove(change.OldPath);
                                }
                            }
                            else
                            {
                                fileChanges[filename] = new FileStat { Filename = filename, CyclomaticComplexity = cyclomaticComplexity, LinesOfCode = linesOfCode };
                            }
                            var folderName = getRootFolder(filename);
                            if (folderChanges.ContainsKey(folderName)) { folderChanges[folderName].ChangeCount++; } else { folderChanges[folderName] = new FileStat { Filename = folderName }; }
                            var usernameFilename = new UsernameFilename { Filename = filename, Username = username };
                            if (userfileChanges.ContainsKey(usernameFilename.DictKey)) { userfileChanges[usernameFilename.DictKey].ChangeCount++; } else { userfileChanges[usernameFilename.DictKey] = new FileStat { Filename = filename, Username = username }; }
                        }
                    }
                }

                foreach (var report in reports)
                {
                    report.Generate(fileChanges, userfileChanges, folderChanges);
                }
            }
        }

        private static string getRootFolder(string filename)
        {
            var folderName = Path.GetDirectoryName(filename);
            int pos = folderName.IndexOf("\\");
            if (pos > -1)
            {
                folderName = folderName.Substring(0, pos);
            }
            if (folderName.Length == 0)
            {
                folderName = ".";
            }
            return folderName;
        }
    }
}
