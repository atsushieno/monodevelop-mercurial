using System;
using System.IO;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class SummaryTests : MergeResolveTests
    {
        [Test]
        [Category("Integration")]
        public void Summary_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Summary());
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_HasNoNewChangesets()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfNewChangesets, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ContainsRawOutput()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.RawOutput, Is.StringContaining("(clean)"));
        }

        [Test]
        [Category("Integration")]
        public void Summary_RepoAtHeadMinusOne_HasOneNewChangeset()
        {
            CreateRepoWithTwoChangesets();
            Repo.Update(0);

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfNewChangesets, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Summary_RepoAtHeadMinusOne_UpdateIsPossible()
        {
            CreateRepoWithTwoChangesets();
            Repo.Update(0);

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.UpdatePossible, Is.True);
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsDefaultBranch()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.Branch, Is.EqualTo("default"));
        }

        [Test]
        [Category("Integration")]
        public void Summary_NewButUncommittedBranchChange_ReturnsNewBranchName()
        {
            Repo.Init();
            Repo.Branch("newbranch");

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.Branch, Is.EqualTo("newbranch"));
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsNullParent()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            CollectionAssert.AreEqual(
                new[]
                {
                    -1,
                }, summary.ParentRevisionNumbers);
        }

        [Test]
        [Category("Integration")]
        public void Summary_RepoAtChangesetZero_ReportsParentIsZero()
        {
            CreateRepoWithTwoChangesets();
            Repo.Update(0);

            RepositorySummary summary = Repo.Summary();

            CollectionAssert.AreEqual(
                new[]
                {
                    0,
                }, summary.ParentRevisionNumbers);
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsNoModifiedFiles()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfModifiedFiles, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Summary_NewFile_ReportsUnknownFile()
        {
            Repo.Init();
            File.WriteAllText(Path.Combine(Repo.Path, "dummy.txt"), "dummy");

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfUnknownFiles, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Summary_ChangeToTrackedFile_ReportsModifiedFile()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", "123", "123", true);
            File.WriteAllText(Path.Combine(Repo.Path, "dummy.txt"), "dummy");

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfModifiedFiles, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsNoUnknownFiles()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfUnknownFiles, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsNoUnresolvedFiles()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfUnresolvedFiles, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Summary_InitializedRepo_ReportsNotInMerge()
        {
            Repo.Init();

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.IsInMerge, Is.False);
        }

        [Test]
        [Category("Integration")]
        public void Summary_MergeWithConflicts_ReportsConflicts()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.NumberOfUnresolvedFiles, Is.EqualTo(1));
        }

        [Test]
        [Category("Integration")]
        public void Summary_Merge_ReportsInMerge()
        {
            CreateRepositoryWithMergeConflicts();
            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            RepositorySummary summary = Repo.Summary();

            Assert.That(summary.IsInMerge, Is.True);
        }

        [Test]
        [Category("Integration")]
        public void Summary_Merge_ReportsCorrectParents()
        {
            CreateRepositoryWithMergeConflicts();

            try
            {
                Repo.Merge(new MergeCommand()
                    .WithMergeTool(MergeTools.InternalMerge));
            }
            catch (NotSupportedException)
            {
                Assert.Inconclusive("Merge tool not supported in this version");
            }

            RepositorySummary summary = Repo.Summary();

            CollectionAssert.AreEqual(new[] { 2, 1 }, summary.ParentRevisionNumbers);
        }

        private void CreateRepoWithTwoChangesets()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", "1", "1", true);
            WriteTextFileAndCommit(Repo, "dummy.txt", "2", "2", false);
        }
    }
}