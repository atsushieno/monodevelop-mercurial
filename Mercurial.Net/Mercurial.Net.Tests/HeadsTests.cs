using System.Linq;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class HeadsTests : SingleRepositoryTestsBase
    {
        [Test]
        [Category("Integration")]
        public void Heads_AfterBranch_ReturnsTwoChangesets()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);
            WriteTextFileAndCommit(Repo, "test2.txt", "dummy", "dummy", true);
            Repo.Update(0);
            WriteTextFileAndCommit(Repo, "test3.txt", "dummy", "dummy", true);
            Changeset[] log = Repo.Heads().ToArray();
            Assert.That(log.Length, Is.EqualTo(2));
        }

        [Test]
        [Category("Integration")]
        public void Heads_OfEmptyRepository_ThrowsMercurialExecutionException()
        {
            Repo.Init();
            Assert.Throws<MercurialExecutionException>(() => Repo.Heads());
        }

        [Test]
        [Category("Integration")]
        public void Heads_OfUninitializedRepository_ThrowsMercurialExecutionException()
        {
            Assert.Throws<MercurialExecutionException>(() => Repo.Heads());
        }

        [Test]
        [Category("Integration")]
        public void Heads_WithSingleCommit_ReturnsTipChangeset()
        {
            Repo.Init();
            WriteTextFileAndCommit(Repo, "test1.txt", "dummy", "dummy", true);
            Changeset[] log = Repo.Heads().ToArray();
            Assert.That(log.Length, Is.EqualTo(1));
        }
    }
}