using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Mercurial.Tests
{
    public class RepositoryTestsBase
    {
        private readonly List<string> _Repositories = new List<string>();
        private readonly List<string> _TempFiles = new List<string>();

        private bool _IsUsingPersistentClient;

        public bool IsUsingPersistentClient
        {
            get
            {
                return _IsUsingPersistentClient;
            }
        }

        protected Repository GetRepository()
        {
            string repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", string.Empty).ToLowerInvariant());
            Directory.CreateDirectory(repoPath);
            _Repositories.Add(repoPath);
            _IsUsingPersistentClient = Environment.GetEnvironmentVariable("PERSISTENTCLIENT") == "1";
            if (IsUsingPersistentClient)
                return new Repository(repoPath, new AutoSwitchingClientFactory());
            return new Repository(repoPath, new NonPersistentClientFactory());
        }

        protected string GetTempFileName()
        {
            string result = Path.GetTempFileName();
            _TempFiles.Add(result);
            return result;
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (string repo in _Repositories)
                DeleteTempDirectory(repo);
            foreach (string path in _TempFiles.Where(File.Exists))
                File.Delete(path);
        }

        private static void DeleteTempDirectory(string path)
        {
            for (int index = 1; index < 5; index++)
            {
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("exception while cleaning up repository directory: " + ex.GetType().Name + ": " + ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }

        protected static void WriteTextFileAndCommit(Repository repo, string fileName, string contents, string commitMessage, bool addRemove)
        {
            File.WriteAllText(Path.Combine(repo.Path, fileName), contents);
            repo.Commit(
                new CommitCommand
                {
                    Message = commitMessage,
                    AddRemove = addRemove,
                });
        }
    }
}