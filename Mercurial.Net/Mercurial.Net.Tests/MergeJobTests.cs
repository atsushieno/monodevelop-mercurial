using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class MergeJobTests : MergeResolveTests
    {
        [Test]
        [Category("Integration")]
        public void StartMerge_NoRepository_ThrowsMercurialExecutionException()
        {
            try
            {
                Repo.StartMerge();
            }
            catch (MercurialExecutionException)
            {
                // success
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void StartMerge_NoChangesetsToMerge_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", "dummy", "dummy", true);

            try
            {
                Repo.StartMerge();
            }
            catch (MercurialExecutionException)
            {
                // success
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void StartMerge_RepoWithoutMergeConflict_ReturnsMergeJobThatIsReadyToCommit()
        {
            CreateRepositoryWithoutMergeConflicts();

            try
            {
                MergeJob job = Repo.StartMerge();
                Assert.That(job.State, Is.EqualTo(MergeJobState.ReadyToCommit));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void StartMerge_RepoWithMergeConflict_ReturnsMergeJobThatHasConflicts()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                MergeJob job = Repo.StartMerge();
                Assert.That(job.State, Is.EqualTo(MergeJobState.HasUnresolvedConflicts));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void StartMerge_RepoWithMergeConflict_ReturnsMergeJobThatListsTheConflictingFile()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                MergeJob job = Repo.StartMerge();
                CollectionAssert.AreEqual(
                    new[]
                {
                    new MergeJobConflict(job, "dummy.txt", MergeConflictState.Unresolved),
                }, job.UnresolvedConflicts);
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void CancelMerge_MergeWithConflicts_RestoresStateOfFile()
        {
            CreateRepositoryWithMergeConflicts();

            MergeJob job;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
                return;
            }
            job.CancelMerge();

            string contents = File.ReadAllText(Path.Combine(Repo.Path, "dummy.txt"));
            const string expected = "1 yyy\n2\n3\n4\n5";

            Assert.That(contents, Is.EqualTo(expected));
        }

        [Test]
        [Category("Integration")]
        public void ResolveConflictThroughMerge_MarksFileAsResolved()
        {
            CreateRepositoryWithMergeConflicts();

            MergeJob job = null;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
            job.UnresolvedConflicts.First().Resolve(new ResolveCommand().WithMergeTool(MergeTools.InternalLocal));

            Assert.That(job.State, Is.EqualTo(MergeJobState.ReadyToCommit));
        }

        [Test]
        [Category("Integration")]
        public void Cleanup_RemovesExtraFiles()
        {
            CreateRepositoryWithMergeConflicts();

            MergeJob job;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
                return;
            }
            job.Cleanup();

            FileStatus[] status = Repo.Status().ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    new FileStatus(FileState.Modified, "dummy.txt"),
                }, status);
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithUnresolvedConflicts_ThrowsInvalidOperationException()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                MergeJob job = Repo.StartMerge();
                Assert.Throws<InvalidOperationException>(() => job.Commit("merged"));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        public void Commit_WithNoUnresolvedConflicts_Commits()
        {
            CreateRepositoryWithoutMergeConflicts();

            int logEntriesBeforeCommit = Repo.Log().Count();
            MergeJob job;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
                return;
            }

            job.Commit("merged");
            int logEntriesAfterCommit = Repo.Log().Count();

            Assert.That(logEntriesAfterCommit, Is.GreaterThan(logEntriesBeforeCommit));
        }

        [Test]
        [Category("Integration")]
        public void Parents_InAMerge_HasCorrectValues()
        {
            CreateRepositoryWithoutMergeConflicts();

            try
            {
                MergeJob job = Repo.StartMerge();
                Assert.That(job.LocalParent.RevisionNumber, Is.EqualTo(2));
                Assert.That(job.OtherParent.RevisionNumber, Is.EqualTo(1));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }
        }

        [Test]
        [Category("Integration")]
        [TestCase(MergeJobConflictSubFile.Base, "dummy.txt.base")]
        [TestCase(MergeJobConflictSubFile.Local, "dummy.txt.local")]
        [TestCase(MergeJobConflictSubFile.Other, "dummy.txt.other")]
        [TestCase(MergeJobConflictSubFile.Current, "dummy.txt")]
        public void MergeJobFile_IndividualFilePathsForAConflict_WithTestCases(MergeJobConflictSubFile subFile, string expectedPath)
        {
            CreateRepositoryWithMergeConflicts();

            MergeJob job;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
                return;
            }

            string path = job[0].GetMergeSubFilePath(subFile);

            Assert.That(path, Is.EqualTo(expectedPath));

            path = job[0].GetFullMergeSubFilePath(subFile);
            expectedPath = Path.Combine(Repo.Path, expectedPath);

            Assert.That(path, Is.EqualTo(expectedPath));
        }

        [Test]
        [Category("Integration")]
        [TestCase(MergeJobConflictSubFile.Base, BaseContents)]
        [TestCase(MergeJobConflictSubFile.Local, LocalChangedContentsWithConflict)]
        [TestCase(MergeJobConflictSubFile.Other, OtherChangedContents)]
        [TestCase(MergeJobConflictSubFile.Current, LocalChangedContentsWithConflict)]
        public void MergeJobFile_IndividualFileContentsForAConflict_WithTestCases(MergeJobConflictSubFile subFile, string expectedContents)
        {
            CreateRepositoryWithMergeConflicts();

            MergeJob job;
            try
            {
                job = Repo.StartMerge();
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
                return;
            }

            string contents = job[0].GetMergeSubFileContentsAsText(subFile);

            Assert.That(contents, Is.EqualTo(expectedContents));
        }
    }
}