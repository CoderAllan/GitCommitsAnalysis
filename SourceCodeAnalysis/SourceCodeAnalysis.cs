using LibGit2Sharp;
using SourceCodeAnalysis.Analysers;
using SourceCodeAnalysis.Interfaces;
using SourceCodeAnalysis.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace SourceCodeAnalysis
{
    public class SourceCodeAnalysis
    {
        private ISystemIO fileHandling;
        private IEnumerable<IReport> reports;

        public SourceCodeAnalysis(ISystemIO fileHandling, IEnumerable<IReport> reports)
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
                var renamedFiles = new Dictionary<string, string>();
                var cyclomaticComplexityCounter = new CyclomaticComplexityCounter();
                var linesOfCodeCalculator = new LinesOfCodeCalculator();
                var simpleLinesOfCodeCalculator = new SimpleLinesOfCodeCalculator();
                foreach (var commit in repo.Commits)
                {
                    var username = commit.Author.Name;
                    var commitDate = commit.Author.When.UtcDateTime;
                    foreach (var parent in commit.Parents)
                    {
                        foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                        {
                            int linesOfCode = 0;
                            int cyclomaticComplexity = 0;
                            var fullPath = Path.Combine(rootFolder, change.Path);
                            if (change.Path != change.OldPath)
                            {
                                if (fileChanges.ContainsKey(change.OldPath))
                                {
                                    fileChanges[change.Path] = fileChanges[change.OldPath];
                                    fileChanges.Remove(change.OldPath);
                                }
                                if (!renamedFiles.ContainsKey(change.OldPath))
                                {
                                    renamedFiles.Add(change.OldPath, change.Path);
                                }
                            }
                            string filename = renamedFiles.ContainsKey(change.OldPath) ? renamedFiles[change.OldPath] : change.Path;

                            if (fileChanges.ContainsKey(filename))
                            {
                                fileChanges[filename].ChangeCount++;
                            }
                            else
                            {
                                if (fileHandling.FileExists(fullPath))
                                {
                                    var fileContents = fileHandling.ReadFileContent(fullPath);
                                    if (change.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                                    {
                                        cyclomaticComplexity = cyclomaticComplexityCounter.Calculate(fileContents);
                                        linesOfCode = linesOfCodeCalculator.Calculate(fileContents);
                                    }
                                    else
                                    {
                                        linesOfCode = simpleLinesOfCodeCalculator.Calculate(fileContents);
                                    }
                                }
                                fileChanges[filename] = new FileStat { Filename = filename, CyclomaticComplexity = cyclomaticComplexity, LinesOfCode = linesOfCode };
                            }
                            fileChanges[filename].CommitDates.Add(commitDate);

                            var folderName = getRootFolder(filename);
                            if (folderChanges.ContainsKey(folderName)) { folderChanges[folderName].ChangeCount++; } else { folderChanges[folderName] = new FileStat { Filename = folderName }; }
                            folderChanges[folderName].CommitDates.Add(commitDate);

                            var usernameFilename = UsernameFilename.GetDictKey(filename, username);
                            if (userfileChanges.ContainsKey(usernameFilename)) { userfileChanges[usernameFilename].ChangeCount++; } else { userfileChanges[usernameFilename] = new FileStat { Filename = filename, Username = username }; }
                            userfileChanges[usernameFilename].CommitDates.Add(commitDate);
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
            int pos = folderName.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
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
