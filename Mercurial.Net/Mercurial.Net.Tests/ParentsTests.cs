using System;
using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class ParentsTests : MergeResolveTests
    {
        [Test]
        [Category("Integration")]
        public void Parents_NoRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Parents());
        }

        [Test]
        [Category("Integration")]
        public void Parents_InitializedRepo_HasNoParents()
        {
            Repo.Init();

            Changeset[] parents = Repo.Parents().ToArray();

            Assert.That(parents, Is.Empty);
        }

        [Test]
        [Category("Integration")]
        public void Parents_RepoAtChangesetZero_ReportsParentIsZero()
        {
            CreateRepoWithTwoChangesets();
            Repo.Update(0);

            Changeset[] parents = Repo.Parents().ToArray();

            Assert.That(parents[0].RevisionNumber, Is.EqualTo(0));
        }

        [Test]
        [Category("Integration")]
        public void Parents_InMerge_ReportsTwoParents()
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

            Changeset[] parents = Repo.Parents().ToArray();

            Assert.That(parents.Length, Is.EqualTo(2));
            Assert.That(parents[0].RevisionNumber, Is.EqualTo(2));
            Assert.That(parents[1].RevisionNumber, Is.EqualTo(1));
        }

        private void CreateRepoWithTwoChangesets()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "dummy.txt", "1", "1", true);
            WriteTextFileAndCommit(Repo, "dummy.txt", "2", "2", false);
        }
    }
}