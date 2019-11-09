using LibGit2Sharp;
using GitCommitsAnalysis.Analysers;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace GitCommitsAnalysis
{
    public class GitCommitsAnalysis
    {
        private ISystemIO fileHandling;
        private IEnumerable<IReport> reports;

        public GitCommitsAnalysis(ISystemIO fileHandling, IEnumerable<IReport> reports)
        {
            this.fileHandling = fileHandling;
            this.reports = reports;
        }

        public void PerformAnalysis(string rootFolder)
        {
            Console.WriteLine("Analysing commits...");
            using (var repo = new Repository(rootFolder))
            {
                var commitsEachDay = new Dictionary<DateTime, int>();
                var fileCommits = new Dictionary<string, FileStat>();
                var folderCommits = new Dictionary<string, FileStat>();
                var userfileCommits = new Dictionary<string, FileStat>();
                var renamedFiles = new Dictionary<string, string>();
                var cyclomaticComplexityCounter = new CyclomaticComplexityCounter();
                var linesOfCodeCalculator = new LinesOfCodeCalculator();
                var simpleLinesOfCodeCalculator = new SimpleLinesOfCodeCalculator();
                foreach (var commit in repo.Commits)
                {
                    var username = commit.Author.Name;
                    var commitDate = commit.Author.When.UtcDateTime.Date;
                    if (commitsEachDay.ContainsKey(commitDate))
                    {
                        commitsEachDay[commitDate]++;
                    }
                    else
                    {
                        commitsEachDay[commitDate] = 1;
                    }
                    foreach (var parent in commit.Parents)
                    {
                        foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                        {
                            int linesOfCode = 0;
                            int cyclomaticComplexity = 0;
                            var fullPath = Path.Combine(rootFolder, change.Path);
                            if (change.Path != change.OldPath)
                            {
                                if (fileCommits.ContainsKey(change.OldPath))
                                {
                                    fileCommits[change.Path] = fileCommits[change.OldPath];
                                    fileCommits.Remove(change.OldPath);
                                }
                                if (!renamedFiles.ContainsKey(change.OldPath))
                                {
                                    renamedFiles.Add(change.OldPath, change.Path);
                                }
                            }
                            string filename = renamedFiles.ContainsKey(change.OldPath) ? renamedFiles[change.OldPath] : change.Path;

                            if (fileCommits.ContainsKey(filename))
                            {
                                fileCommits[filename].CommitCount++;
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
                                fileCommits[filename] = new FileStat { Filename = filename, CyclomaticComplexity = cyclomaticComplexity, LinesOfCode = linesOfCode };
                            }
                            fileCommits[filename].CommitDates.Add(commitDate);

                            var folderName = getRootFolder(filename);
                            if (folderCommits.ContainsKey(folderName)) { folderCommits[folderName].CommitCount++; } else { folderCommits[folderName] = new FileStat { Filename = folderName }; }
                            folderCommits[folderName].CommitDates.Add(commitDate);

                            var usernameFilename = UsernameFilename.GetDictKey(filename, username);
                            if (userfileCommits.ContainsKey(usernameFilename)) { userfileCommits[usernameFilename].CommitCount++; } else { userfileCommits[usernameFilename] = new FileStat { Filename = filename, Username = username }; }
                            userfileCommits[usernameFilename].CommitDates.Add(commitDate);
                        }
                    }
                }

                foreach (var report in reports)
                {
                    report.Generate(fileCommits, userfileCommits, folderCommits, commitsEachDay);
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
