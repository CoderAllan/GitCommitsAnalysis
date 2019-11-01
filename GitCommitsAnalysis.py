import os
import operator
from git import Repo

# Script inspired by this youtube video: https://www.youtube.com/watch?v=a74UkJxKWVM&t=2450s

# GitPython code from example at https://www.fullstackpython.com/blog/first-steps-gitpython.html

class FileStats:
    def __init__(self, filename: str, username: str, numberOfChanges: int):
        self.Filename = filename
        self.Username = username
        self.NumberOfChanges = numberOfChanges


fileChanges = {}
fileChangesByUser = {}


def print_commit(commit):
    print('----')
    print(str(commit.hexsha))
    print("\"{}\" by {} ({})".format(commit.summary,
                                     commit.author.name,
                                     commit.author.email))
    print(str(commit.authored_datetime))
    print(str("count: {} and size: {}".format(commit.count(),
                                              commit.size)))
    print(commit.tree)


def calculateNumberOfChanges(commit):
    for filename in commit.stats.files:
        # print(str("- {}".format(file)))
        if filename in fileChanges:
            fileChanges[filename].NumberOfChanges += 1
        else:
            fileChanges[filename] = FileStats(filename, "", 1)
        authorname = commit.author.name
        key = str(filename + ', ' + authorname)
        if key in fileChangesByUser:
            fileChangesByUser[key].NumberOfChanges += 1
        else:
            fileChangesByUser[key] = FileStats(filename, authorname, 1)

def print_repository(repo):
    print('Repo description: {}'.format(repo.description))
    print('Repo active branch is {}'.format(repo.active_branch))
    for remote in repo.remotes:
        print('Remote named "{}" with URL "{}"'.format(remote, remote.url))
    print('Last commit for repo is {}.'.format(str(repo.head.commit.hexsha)))


if __name__ == "__main__":
    repo_path = "."  # os.getenv('GIT_REPO_PATH')
    # Repo object used to programmatically interact with Git repositories
    repo = Repo(repo_path)
    # check that the repository loaded correctly
    if not repo.bare:
        print('Repo at {} successfully loaded.'.format(repo_path))
        print_repository(repo)
        # create list of commits then print some of them to stdout
        commits = list(repo.iter_commits('master'))
        numberOfCommits = len(commits)
        print("Analysing commits: {}".format(numberOfCommits))
        for commit in commits:
            # print_commit(commit)
            calculateNumberOfChanges(commit)
            pass
        print("----------------------------------------")
        for filename in (sorted(fileChanges, key=lambda filename: fileChanges[filename].NumberOfChanges, reverse=True)):
            print(
                "- {}: {}".format(fileChanges[filename].Filename, fileChanges[filename].NumberOfChanges))
        for filenameAuthor in (sorted(fileChangesByUser, key=lambda filenameAuthor: fileChangesByUser[filenameAuthor].NumberOfChanges, reverse=True)):
            if(fileChangesByUser[filenameAuthor].NumberOfChanges > 2):
                print("- {}, {}: {}".format(fileChangesByUser[filenameAuthor].Filename,
                                        fileChangesByUser[filenameAuthor].Username,
                                        fileChangesByUser[filenameAuthor].NumberOfChanges))
                filenameAuthor
                
    else:
        print('Could not load repository at {} :('.format(repo_path))
