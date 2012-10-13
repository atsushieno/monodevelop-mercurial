using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class TipTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Tip_InitializedRepositoryWithChangeset_ReturnsCommittedChangeset()
        {
            const string commitMessage = "commit message test 123";

            Repo.Init();
            WriteTextFileAndCommit(Repo, "test.txt", "dummy", commitMessage, true);
            Changeset changeset = Repo.Tip();

            Assert.That(changeset.CommitMessage, Is.EqualTo(commitMessage));
        }

        [Test]
        [Category("Integration")]
        public void Tip_InitializedRepositoryWithoutChangesets_ReturnsNullChangeset()
        {
            Repo.Init();
            Changeset changeset = Repo.Tip();

            Assert.That(changeset.Hash, Is.EqualTo(new string('0', 40)));
        }

        [Test]
        [Category("Integration")]
        public void Tip_UninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Tip());
        }
    }
}