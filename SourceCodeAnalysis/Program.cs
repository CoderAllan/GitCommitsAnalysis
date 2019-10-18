using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LibGit2Sharp;

namespace SourceCodeAnalysis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var repo = new Repository("D:\\Src\\AssetValueService"))
            {
                var fileChanges = new Dictionary<string, FileStat>();
                var folderChanges = new Dictionary<string, FileStat>();
                var userfileChanges = new Dictionary<string, FileStat>();
                foreach (var commit in repo.Commits)
                {
                    //Console.WriteLine($"{counter} commit: {commit.Author.Name}, {commit.MessageShort}");
                    var username = commit.Author.Name;
                    foreach (var parent in commit.Parents)
                    {
                        foreach (TreeEntryChanges change in repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                        {
                            var filename = change.OldPath;
                            if (fileChanges.ContainsKey(filename)) { fileChanges[filename].ChangeCount++; } else { fileChanges[filename] = new FileStat { Filename = filename}; }
                            var folderName = getRootFolder(filename);
                            if (folderChanges.ContainsKey(folderName)) { folderChanges[folderName].ChangeCount++; } else { folderChanges[folderName] = new FileStat { Filename = folderName }; }
                            var usernameFilename = new UsernameFilename { Filename = filename, Username = username };
                            if (userfileChanges.ContainsKey(usernameFilename.DictKey)) { userfileChanges[usernameFilename.DictKey].ChangeCount++; } else { userfileChanges[usernameFilename.DictKey] = new FileStat { Filename = filename, Username = username }; }
                        }
                    }
                }
                var fileChangesList = fileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename);
                var userfileChangesList = userfileChanges.Values.OrderByDescending(fc => fc.ChangeCount).ThenBy(fc => fc.Filename).ThenBy(fc => fc.Username);
                var totalChanges = fileChangesList.Sum(fc => fc.ChangeCount);
                Console.WriteLine($"Changes: {totalChanges}");
                foreach (var fileChange in fileChangesList.Take(50))
                {
                    Console.WriteLine($"{fileChange.Filename}: {fileChange.ChangeCount}");
                    foreach (var userfileChange in userfileChangesList.Where(ufc => ufc.Filename == fileChange.Filename))
                    {
                        var username = string.Format("{0,20}", userfileChange.Username);
                        var changeCount = string.Format("{0,3}", userfileChange.ChangeCount);
                        var percentage = string.Format("{0,5:##.00}", ((double)userfileChange.ChangeCount / (double)fileChange.ChangeCount) * 100);
                        Console.WriteLine($"    {username}: {changeCount} ({percentage}%)");
                    }
                }
                Console.WriteLine("-------");
                var folderChangesList = folderChanges.Values.OrderByDescending(fc => fc.ChangeCount);
                foreach(var folder in folderChangesList.Take(25))
                {
                    var folderName = string.Format("{0,50}", folder.Filename);
                    var changeCount = string.Format("{0,5}", folderChanges[folder.Filename].ChangeCount);
                    var percentage = string.Format("{0,5:##.00}", ((double)folderChanges[folder.Filename].ChangeCount / (double)totalChanges) * 100);
                    Console.WriteLine($"{folderName}: {changeCount} ({percentage}%)");
                }
            }
        }

       private static string getRootFolder(string filename)
        {
            var folderName = Path.GetDirectoryName(filename);
            int pos = folderName.IndexOf("\\");
            if(pos > -1)
            {
                folderName = folderName.Substring(0, pos);
            }
            if(folderName.Length == 0)
            {
                folderName = ".";
            }
            return folderName;
        }

        private class UsernameFilename
        {
            public string Username { get; set; }
            public string Filename { get; set; }

            public string DictKey { get { return Filename + Username; } }
        }

        private class FileStat : UsernameFilename
        {
            public int ChangeCount { get; set; } = 1;
        }
    }

}
