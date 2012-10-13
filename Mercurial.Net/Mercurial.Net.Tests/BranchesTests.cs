using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class BranchesTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Branches_NoRepo_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Branches());
        }

        [Test]
        [Category("Integration")]
        public void Branches_InitializedRepo_ReturnsNoBranches()
        {
            Repo.Init();

            BranchHead[] branches = Repo.Branches().ToArray();

            Assert.That(branches, Is.Empty);
        }

        [Test]
        [Category("Integration")]
        public void Branches_OneChangesetPresent_ReturnsDefaultBranch()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "test", "test", true);

            BranchHead[] branches = Repo.Branches().ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    new BranchHead(0, "default"),
                }, branches);
        }
    }
}