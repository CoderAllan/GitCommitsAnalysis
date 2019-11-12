using LibGit2Sharp;
using GitCommitsAnalysis.Analysers;
using GitCommitsAnalysis.Interfaces;
using GitCommitsAnalysis.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zu.TypeScript;

namespace GitCommitsAnalysis
{
    public class GitCommitsAnalysis
    {
        private ISystemIO fileHandling;
        private IEnumerable<IReport> reports;
        private Options options;

        public GitCommitsAnalysis(ISystemIO fileHandling, IEnumerable<IReport> reports, Options options)
        {
            this.fileHandling = fileHandling;
            this.reports = reports;
            this.options = options;
        }

        public void PerformAnalysis(string rootFolder)
        {
            Console.WriteLine("Analysing commits...");
            using (var repo = new Repository(rootFolder))
            {
                var analysis = new Analysis();
                var renamedFiles = new Dictionary<string, string>();
                var cyclomaticComplexityCounter = new CyclomaticComplexityCounter();
                var linesOfCodeCalculator = new LinesOfCodeCalculator();
                var typeScriptAst = new TypeScriptAST();
                foreach (var commit in repo.Commits)
                {
                    var username = commit.Author.Name;
                    var commitDate = commit.Author.When.UtcDateTime.Date;
                    UpdateAnalysisCommitDates(analysis, commitDate);
                    IncDictionaryValue(analysis.CommitsEachDay, commitDate);
                    foreach (var parent in commit.Parents)
                    {
                        var patch = repo.Diff.Compare<Patch>(parent.Tree, commit.Tree);
                        IncDictionaryValue(analysis.LinesOfCodeAddedEachDay, commitDate, patch.LinesAdded);
                        IncDictionaryValue(analysis.LinesOfCodeDeletedEachDay, commitDate, patch.LinesDeleted);
                        foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                        {
                            int linesOfCode = 0;
                            int cyclomaticComplexity = 0;
                            int methodCount = 0;
                            var fullPath = Path.Combine(rootFolder, change.Path);
                            if (change.Path != change.OldPath)
                            {
                                if (analysis.FileCommits.ContainsKey(change.OldPath))
                                {
                                    analysis.FileCommits[change.Path] = analysis.FileCommits[change.OldPath];
                                    analysis.FileCommits.Remove(change.OldPath);
                                }
                                if (!renamedFiles.ContainsKey(change.OldPath))
                                {
                                    renamedFiles.Add(change.OldPath, change.Path);
                                }
                            }
                            string filename = renamedFiles.ContainsKey(change.OldPath) ? renamedFiles[change.OldPath] : change.Path;
                            var fileType = Path.GetExtension(filename);
                            if (IgnoreFiletype(fileType))
                            {
                                break;
                            }

                            if (analysis.FileCommits.ContainsKey(filename))
                            {
                                analysis.FileCommits[filename].CommitCount++;
                            }
                            else
                            {
                                if (fileHandling.FileExists(fullPath))
                                {
                                    var fileContents = fileHandling.ReadFileContent(fullPath);
                                    if (change.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var syntaxTree = CodeAnalyser.GetSyntaxTree(fileContents);
                                        var methodDeclarationNode = CodeAnalyser.GetMethodDeclarationSyntaxe(syntaxTree);
                                        cyclomaticComplexity = cyclomaticComplexityCounter.Calculate(methodDeclarationNode, syntaxTree);
                                        methodCount = MethodCounter.Calculate(methodDeclarationNode);
                                    }
                                    else if (change.Path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                                    {
                                        methodCount = MethodCounter.Calculate(typeScriptAst, fileContents);
                                    }
                                    linesOfCode = linesOfCodeCalculator.Calculate(fileContents);
                                    analysis.LinesOfCodeAnalysed += linesOfCode;
                                }
                                analysis.FileCommits[filename] = new FileStat { Filename = filename, CyclomaticComplexity = cyclomaticComplexity, LinesOfCode = linesOfCode, MethodCount = methodCount };
                                IncDictionaryValue(analysis.FileTypes, fileType);
                            }
                            analysis.FileCommits[filename].CommitDates.Add(commitDate);

                            var folderName = getRootFolder(filename);
                            if (analysis.FolderCommits.ContainsKey(folderName)) { analysis.FolderCommits[folderName].CommitCount++; } else { analysis.FolderCommits[folderName] = new FileStat { Filename = folderName }; }
                            analysis.FolderCommits[folderName].CommitDates.Add(commitDate);

                            var usernameFilename = UsernameFilename.GetDictKey(filename, username);
                            if (analysis.UserfileCommits.ContainsKey(usernameFilename)) { analysis.UserfileCommits[usernameFilename].CommitCount++; } else { analysis.UserfileCommits[usernameFilename] = new FileStat { Filename = filename, Username = username }; }
                            analysis.UserfileCommits[usernameFilename].CommitDates.Add(commitDate);
                        }
                    }
                }
                analysis.AnalysisTime = (DateTime.UtcNow.Ticks - analysis.CreatedDate.Ticks) / 10000; // Analysis time in miliseconds
                foreach (var report in reports)
                {
                    report.Generate(analysis);
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

        private static void IncDictionaryValue<T>(Dictionary<T, int> dictionary, T key, int increment = 1)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += increment;
            }
            else
            {
                dictionary[key] = increment;
            }
        }

        private static void UpdateAnalysisCommitDates(Analysis analysis, DateTime commitDate)
        {
            if(commitDate < analysis.FirstCommitDate)
            {
                analysis.FirstCommitDate = commitDate;
            } 
            else if (commitDate > analysis.LatestCommitDate)
            {
                analysis.LatestCommitDate = commitDate;
            }
        }

        private bool IgnoreFiletype(string fileExtension)
        {
            if (!string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = fileExtension.Substring(1);
                if (options.IgnoredFiletypes != null && options.IgnoredFiletypes.Any() && options.IgnoredFiletypes.Contains(fileExtension))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
